namespace TimeForge.ViewModels.Project;

public class PagedProjectViewModel
{
    public IEnumerable<ProjectViewModel> Projects { get; set; }


    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNexPage => CurrentPage < TotalPages;
}