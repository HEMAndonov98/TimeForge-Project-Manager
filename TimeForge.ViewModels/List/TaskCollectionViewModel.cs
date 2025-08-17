namespace TimeForge.ViewModels.List;

public class TaskCollectionViewModel
{
    public string Id { get; set; }
    public string Title { get; set; }

    public List<TaskItemViewModel> Tasks { get; set; }
}