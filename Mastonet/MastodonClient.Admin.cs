using Mastonet.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mastonet;

public enum AdminAccountOrigin
{
    Local,
    Remote
}

public enum AdminAccountStatus
{
    Active,
    Pending,
    Disabled,
    Silenced,
    Suspended
}

public enum AdminActionType
{
    None,
    Sensitive,
    Disable,
    Silence,
    Suspend
}

public enum AdminBlockDomainAction
{
    Noop,
    Silence,
    Suspend
}

public partial class MastodonClient
{
    public Task<MastodonList<AdminAccount>> GetAdminAccounts(ArrayOptions? options = null, AdminAccountOrigin? origin = null,
        AdminAccountStatus? status = null, string? permissions = null, string? invitedBy = null, string? username = null,
        string? displayName = null, string? byDomain = null, string? email = null, string? userIp = null)
    {
        const string url = "/api/v2/admin/accounts";
        var queryParams = "";
        queryParams = AddQueryStringParam(queryParams, "origin", origin?.ToString().ToLowerInvariant());
        queryParams = AddQueryStringParam(queryParams, "status", status?.ToString().ToLowerInvariant());
        queryParams = AddQueryStringParam(queryParams, "permissions", permissions);
        queryParams = AddQueryStringParam(queryParams, "invited_by", invitedBy);
        queryParams = AddQueryStringParam(queryParams, "username", username);
        queryParams = AddQueryStringParam(queryParams, "display_name", displayName);
        queryParams = AddQueryStringParam(queryParams, "by_domain", byDomain);
        queryParams = AddQueryStringParam(queryParams, "email", email);
        queryParams = AddQueryStringParam(queryParams, "ip", userIp);
        if (options != null)
        {
            var concatChar = GetQueryStringConcatChar(queryParams);
            queryParams += concatChar + options.ToQueryString();
        }

        return GetMastodonList<AdminAccount>(url + queryParams);
    }

    public Task<AdminAccount> DeleteAccount(string accountId)
    {
        return Delete<AdminAccount>($"/api/v1/admin/accounts/{accountId}");
    }

    public async Task PerformAccount(string accountId, AdminActionType action,
        string? reportId = null, string? warningPresetId = null, string? text = null, bool send_email_notification = false)
    {
        var data = new Dictionary<string, string>
        {
            { "type", action.ToString().ToLowerInvariant() },
            { "send_email_notification", send_email_notification.ToString().ToLowerInvariant() },
        };
        if (reportId != null)
        {
            data.Add("report_id", reportId);
        }
        if (warningPresetId != null)
        {
            data.Add("warning_preset_id", warningPresetId);
        }
        if (text != null)
        {
            data.Add("text", text);
        }
        await Post($"/api/v1/admin/accounts/{accountId}/action", data);
    }

    public Task AdminBlockDomain(string domain, AdminBlockDomainAction action = AdminBlockDomainAction.Silence,
        bool rejectMedia = false, bool rejectReports = false,
        string? privateComment = null, string? publicComment = null,
        bool obfuscate = false)
    {
        var data = new Dictionary<string, string>
        {
            { "domain", domain },
            { "severity", action.ToString().ToLowerInvariant() },
            { "reject_media", rejectMedia.ToString().ToLowerInvariant() },
            { "reject_reports", rejectReports.ToString().ToLowerInvariant() },
            { "obfuscate", obfuscate.ToString().ToLowerInvariant() },
        };
        if (privateComment != null)
        {
            data.Add("private_comment", privateComment);
        }
        if (publicComment != null)
        {
            data.Add("public_comment", publicComment);
        }
        return Post($"/api/v1/admin/domain_blocks", data);
    }
}
