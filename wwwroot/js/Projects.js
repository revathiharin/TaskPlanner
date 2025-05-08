function createTask(projectId, taskTitle) {
    $.ajax({
        url: '/Tasks/CreateTask',
        type: 'POST',
        data: { projectId: projectId, title: taskTitle },
        success: function () {
            console.log("Task created successfully.");
            // Reload tasks for the selected project
            loadTasks(projectId);
        },
        error: function () {
            console.error("Error creating task.");
        }
    });
}

// Function to sort tasks based on criteria
function sortTasks(tasks, sortBy) {
    let sortedTasks;

    if (sortBy === 'A-Z') {
        sortedTasks = tasks.sort((a, b) => a.t.title.localeCompare(b.t.title));
    } else if (sortBy === 'Z-A') {
        sortedTasks = tasks.sort((a, b) => b.t.title.localeCompare(a.t.title));
    } else if (sortBy === 'Completed') {
        sortedTasks = tasks.sort((a, b) => {
            return a.t.isCompleted - b.t.isCompleted || a.t.sortOrder - b.t.sortOrder;
        });
    } else if (sortBy === 'Manual') {
        sortedTasks = tasks.sort((a, b) => a.t.sortOrder - b.t.sortOrder);
    }

    return sortedTasks;
}

function loadTasks(projectId) {
    $.ajax({
        url: '/Tasks/GetTasks',
        type: 'GET',
        data: { projectId: projectId },
        success: function (response) {
            const sortBy = response.sortBy.sortBy;
            $("#sortBy").val(sortBy);
            const tasksTemp = response.tasks;
            const sortedTasks = sortTasks(tasksTemp, sortBy);
            console.log("loadTasks", sortBy, tasksTemp, sortedTasks);
            
            console.log('/Tasks/GetTasks', sortedTasks);
            $("#taskList").empty(); // Clear the current task list
            sortedTasks.forEach(task => {

                console.log(task.t.title);
                $("#taskList").append(`
                    <li data-id="${task.t.id}" class="task-item">
                        <input type="checkbox" class="task-completion" ${task.t.isCompleted ? 'checked' : ''} />
                        ${task.t.title}
                    </li>
                `);
            });

            // Enable sortable only if sortBy is 'Manual'
            if (sortBy === 'Manual') {
                $("#taskList").sortable("enable");
            } else {
                $("#taskList").sortable("disable");
            }
        },
        error: function () {
            console.error("Error loading tasks for project ID:", projectId);
        }
    });
}


function enableProjectDroppable() {
    $("#projectList li").each(function () {
        $(this).droppable({
            accept: ".task-item",
            hoverClass: "highlight-drop",
            drop: function (event, ui) {
                console.log('MoveTaskToProject');
                const newProjectId = $(this).data("id");
                const taskId = ui.draggable.data("id");

                $.ajax({
                    url: '/Tasks/MoveTaskToProject',
                    type: 'POST',
                    data: { taskId, newProjectId },
                    success: function (tasks) {
                        console.log('MoveTaskToProject Success');
                        const selectedProjectId = $("#projectId").val();
                        //removeTask(taskId,tasks);
                        if (selectedProjectId) {
                            loadTasks(selectedProjectId);
                        }
                    },
                    error: function () {
                        console.log("Error MoveTaskToProject");
                    }
                });
            }
        });
    });
}


$(function () {
    // Project click
    $("#projectList").on("click", "li", function () {
        const projectId = $(this).data("id");
        $("#projectId").val(projectId);
        $("#projectList li").removeClass("selected");
        $(this).addClass("selected");
        $("#addTaskForm").show();
        // Show main content when a project is selected
        $("main.main-content").show();
        console.log("Clicked");
        loadTasks(projectId);
        
    });

    $('#sortBy').change(function () {

        const select = $('#sortBy');
        const value = select.val();
        const projectId = $("#projectId").val();
        var data = JSON.stringify({ projectId: projectId, sortBy: value });
        console.log("Databased : ", projectId, value);
        //UpdateSortBy(projectId, value); // Call the UpdateSortBy function when the value changes
        $.ajax({
            url: '/Projects/UpdateSortBy', // Adjust the controller name as needed
            type: 'POST',
            contentType: 'application/json',
            data: data,
            success: function (response) {
                console.log('Database updated successfully:', response);
                loadTasks(projectId);
            },
            error: function (xhr, status, error) {
                console.error('Error updating database:', error);
            }
        });
    });

    // Task completion
    $(document).on("change", ".task-completion", function () {
        const taskId = $(this).closest("li").data("id");
        const isCompleted = $(this).is(":checked");
        $.post('/Tasks/UpdateCompletion', { id: taskId, isCompleted });
    });

    
        // Drag-and-drop with sortable
        $("#taskList").sortable({
            placeholder: "ui-state-highlight",
            helper: "clone",
            revert: true,
            update: function () {
                const updatedTasks = [];
                const projectId = $("#projectId").val();

                $("#taskList li").each(function (index) {
                    const taskId = $(this).data("id");
                    updatedTasks.push({
                        id: taskId,
                        sortOrder: index,
                        projectId: projectId
                    });
                });

                $.ajax({
                    url: '/Tasks/ReorderAndMoveTasks',
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ tasks: updatedTasks }),
                    success: function () {
                        console.log("Updated task order and project assignment.");
                    },
                    error: function () {
                        console.error("Error updating task data.");
                    }
                });
            }
        }).disableSelection();
    
    // On page load, check if no project is selected
    if ($("#projectList li.selected").length === 0) {
        $("main.main-content").hide();
    }
    enableProjectDroppable();

    // Handle task creation form submission
    $("#addTaskForm").on("submit", function (e) {
        e.preventDefault();
        const projectId = $("#projectId").val();
        const taskTitle = $("#taskTitle").val(); // Assuming you have an input with id 'taskTitle'

        if (taskTitle) {
            createTask(projectId, taskTitle);
            $("#taskTitle").val(''); // Clear the input after creation
        } else {
            alert("Please enter a task title.");
        }
    });
});