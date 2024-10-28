using TaikoWebUI.Pages.Dialogs;
namespace TaikoWebUI.Components;

public partial class ChallengeCompe
{
    [Parameter] public ChallengeCompetition? ChallengeCompetition { get; set; }
    [Parameter] public int Baid { get; set; }
    [Parameter] public EventCallback<ChallengeCompetition> Refresh { get; set; }
    [Parameter] public Dictionary<uint, MusicDetail>? MusicDetailDictionary { get; set; } = null;
    [Parameter] public string? SongNameLanguage { get; set; } = null;

    protected override async Task OnInitializedAsync()
    {
        base.OnInitialized();
        SongNameLanguage ??= await LocalStorage.GetItemAsync<string>("songNameLanguage");
        MusicDetailDictionary ??= await GameDataService.GetMusicDetailDictionary();
    }

    private string GetSongInfo(ChallengeCompetitionSong song)
    {
        MusicDetailDictionary.ThrowIfNull();
        song.MusicDetail.ThrowIfNull();

        var songName = GameDataService.GetMusicNameBySongId(MusicDetailDictionary, song.MusicDetail.SongId, SongNameLanguage);
        if (song.BestScores.Any(bs => bs.Baid == Baid))
        {
            return songName + " (" + Localizer["Played"] + ")";
        }
        return songName;
    }

    private bool SelfHoldedChallengeCompetiton()
    {
        return ChallengeCompetition?.Baid == Baid || AuthService.IsAdmin;
    }

    private bool ChallengeNeedAnswer()
    {
        return !AuthService.IsAdmin && ChallengeCompetition?.State == CompeteState.Waiting && ChallengeCompetition?.Baid != Baid;
    }

    private bool ParticipatedChallengeCompetition()
    {
        return ChallengeCompetition?.Participants?.Find(p => p.Baid == Baid) != null;
    }

    private bool CanParticipateChallengeCompetition()
    {
        return ChallengeCompetition?.CreateTime < DateTime.Now && DateTime.Now < ChallengeCompetition?.ExpireTime && !ParticipatedChallengeCompetition();
    }

    private string FormatChallengeTitle(string template)
    {
        return template
            .Replace("{From}", ChallengeCompetition?.Holder?.MyDonName)
            .Replace("{To}", ChallengeCompetition?.Participants?.Find(p => p.Baid != ChallengeCompetition?.Baid)?.UserInfo?.MyDonName);
    }

    private async Task AnswerChallenge(bool accept)
    {
        if (ChallengeCompetition == null || ChallengeCompetition.State != CompeteState.Waiting) return;
        var url = accept ? $"api/ChallengeCompeteManage/{Baid}/acceptChallenge/{ChallengeCompetition.CompId}" : $"api/ChallengeCompeteManage/{Baid}/rejectChallenge/{ChallengeCompetition.CompId}";
        var response = await Client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            await DialogService.ShowMessageBox(
                Localizer["Error"],
                Localizer["Request Error"],
                Localizer["Dialog OK"], null, null, new DialogOptions { DisableBackdropClick = true });
            return;
        }
        await Refresh.InvokeAsync(ChallengeCompetition);

        ChallengeCompetition.State = accept ? CompeteState.Normal : CompeteState.Rejected;
    }

    private async Task AnswerCompete()
    {
        if (ChallengeCompetition == null) return;
        var response = await Client.GetAsync($"api/ChallengeCompeteManage/{Baid}/joinCompete/{ChallengeCompetition.CompId}");
        if (!response.IsSuccessStatusCode)
        {
            await DialogService.ShowMessageBox(
                Localizer["Error"],
                Localizer["Request Error"],
                Localizer["Dialog OK"], null, null, new DialogOptions { DisableBackdropClick = true });
            return;
        }
        await Refresh.InvokeAsync(ChallengeCompetition);
    }
}