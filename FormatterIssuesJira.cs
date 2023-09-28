using static IssuesJiraModel;

public class FormatterIssuesJira
{
    const string _STATUS = "status";
    const string _SPRINT = "Sprint";
    const string _FORMATDATE = "yyyy-MM-dd";

    // Return only work days
    private static int GetWorkingDays(DateTime dateFrom, DateTime dateTo)
    {
        var dayDifference = (int)dateTo.Subtract(dateFrom).TotalDays;
        return Enumerable
            .Range(1, dayDifference)
            .Select(x => dateFrom.AddDays(x))
            .Count(x => x.DayOfWeek != DayOfWeek.Saturday && x.DayOfWeek != DayOfWeek.Sunday);
    }

    // Return sprint 
    private static string? GetSprintsIssues(IOrderedEnumerable<Histories> itemIssuesChangelogHistories)
    {
        var itemIssuesChangelogHistoriesFiltered = itemIssuesChangelogHistories.Where(x => x.items.Any(x => x.field == _SPRINT && !string.IsNullOrEmpty(x.toString)));

        return itemIssuesChangelogHistoriesFiltered.FirstOrDefault()?.items.FirstOrDefault()?.toString;
    }

    // Return issues that change status
    Func<Items, bool> transictionStatus = x => x.field == _STATUS && x.fromString != x.toString;

    // Format body return by jira at struct object
    public List<IssuesResult> GetIssuesResult(IssuesJira issuesJira)
    {
        var issuesResultList = new List<IssuesResult>();

        foreach (var itemIssues in issuesJira.issues.OrderBy(x => x.id))
        {
            var itemIssuesChangelogHistories = itemIssues.changelog.histories.OrderBy(x => x.created);

            var issuesResult = new IssuesResult
            {
                Id = itemIssues?.id,
                Key = itemIssues?.key,
                Summary = itemIssues?.fields?.summary,
                Assigned = itemIssues?.fields?.assignee?.displayName,
                DateResolved = DateTime.SpecifyKind(Convert.ToDateTime(itemIssues?.fields?.resolutiondate), DateTimeKind.Utc).ToString(_FORMATDATE),
                Sprint = GetSprintsIssues(itemIssuesChangelogHistories)
            };

            if (itemIssues == null)
                continue;

            string dateChangeStatusOld = string.Empty;

            // Build history status issue and cycletimes
            var itemIssuesChangelogHistoriesFiltered = itemIssuesChangelogHistories.Where(x => x.items.Any(transictionStatus));
            foreach (var itemHistories in itemIssuesChangelogHistoriesFiltered)
            {
                
                //Get only items with status diff    
                var itemsStatus = itemHistories?.items.Where(transictionStatus);
                if (itemsStatus == null || itemsStatus.Count() == 0)
                    continue;

                // Prepare dates to calculate cycletimes
                var dateChangeStatus = DateTime.SpecifyKind(Convert.ToDateTime(itemHistories?.created), DateTimeKind.Utc);
                var dateFrom = !string.IsNullOrEmpty(dateChangeStatusOld) ? Convert.ToDateTime(dateChangeStatusOld) : DateTime.MinValue;
                var dateTo = Convert.ToDateTime(dateChangeStatus.ToString(_FORMATDATE));

                //Set object to issues history and calculate cycletimes
                var issuesResultHistories = new IssuesResultHistories
                {
                    UserKey = itemHistories?.author?.name,
                    UserName = itemHistories?.author?.displayName,
                    DateChangeStatus = dateChangeStatus.ToString(_FORMATDATE),
                    CycleTime = (dateFrom != DateTime.MinValue) ? (int)dateTo.Subtract(dateFrom).TotalDays : 0,
                    CycleTimeWorkDays = (dateFrom != DateTime.MinValue) ? GetWorkingDays(dateFrom, dateTo) : 0
                };

                foreach (var items in itemsStatus)
                {
                    issuesResultHistories.FromStatus = items.fromString;
                    issuesResultHistories.ToStatus = items.toString;
                }

                issuesResult.IssuesResultHistories.Add(issuesResultHistories);

                dateChangeStatusOld = dateChangeStatus.ToString(_FORMATDATE);

            }
            issuesResultList.Add(issuesResult);
        }
        return issuesResultList;
    }
}