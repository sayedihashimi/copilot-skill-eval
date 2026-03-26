namespace SparkEvents.Pages.Shared;

public class PaginationModel
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public Dictionary<string, string?> QueryParameters { get; set; } = new();

    public PaginationModel() { }

    public PaginationModel(int currentPage, int totalItems, int pageSize, string baseUrl, Dictionary<string, string?>? queryParams = null)
    {
        CurrentPage = currentPage;
        TotalItems = totalItems;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        BaseUrl = baseUrl;
        QueryParameters = queryParams ?? new();
    }

    public string GetPageUrl(int page)
    {
        var queryParams = new Dictionary<string, string?>(QueryParameters)
        {
            ["pageNumber"] = page.ToString()
        };

        var queryString = string.Join("&", queryParams
            .Where(kv => !string.IsNullOrEmpty(kv.Value))
            .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value!)}"));

        return string.IsNullOrEmpty(queryString) ? BaseUrl : $"{BaseUrl}?{queryString}";
    }
}
