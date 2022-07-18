using System.Collections;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine;
using System.Linq;
using System.IO;
using TMPro;

public class CutsceneParser : MonoBehaviour
{
    public VideoPlayer cutscenePlayer;
    public RawImage rawImage;
    public TextMeshProUGUI subtitleOfCutscene;
    public string[] allLinesOfSubtitle;
    public string path, cutFilename;
    Dictionary<TimeSpan, string> contentsPars;
    bool canSkipped = true, play = false;
    string text = "";

    private void Update() {
        if (play) {
            if (contentsPars != null)
                contentsPars.TryGetValue(new TimeSpan(0, 0, Convert.ToInt32(cutscenePlayer.clockTime)), out text);
            subtitleOfCutscene.text = text;
        }
    }

    public void ParseCutscene(string path, bool autoPlay = false) {
        CutsceneDataInternal cutsceneData = JsonConvert.DeserializeObject<CutsceneDataInternal>(File.ReadAllText(Path.Combine(path, "cutscene.json")));
        CutsceneData cutscene = cutsceneData.cutscene;

        if (cutscene.HasSubtitles) {
            Dictionary<string, string> subtitlesFileName = cutscene.SubtitlesFileName;
            List<string> langs = subtitlesFileName.Keys.ToList();
            Dictionary<int, string> subtitleContents = null;
            if (langs.Contains(Application.systemLanguage.ToString())) {
                allLinesOfSubtitle = File.ReadAllLines(Path.Combine(path, "subtitles", subtitlesFileName[Application.systemLanguage.ToString()]));
                subtitleContents = SubtitleParser(allLinesOfSubtitle);
            }
            Dictionary<string, string> timeStamps = cutscene.TimeStamps;
            contentsPars = TimeStampCalculator(timeStamps, subtitleContents);
            subtitleOfCutscene.gameObject.SetActive(true);
        }
        this.path = path;
        cutFilename = cutscene.CutsceneFileName;
        if (autoPlay)
            PlayCutscene();
    }

    public void PlayCutscene() {
        cutscenePlayer.gameObject.SetActive(true);
        cutscenePlayer.url = "file://" + Path.Combine(path, cutFilename);
        cutscenePlayer.Play();
        Song.instance.cutsceneIsPlaying = true;
        rawImage.gameObject.SetActive(true);
        play = true;
        StartCoroutine(Enum());
    }

    public IEnumerator Enum() {
        yield return new WaitForSeconds(.5f);
        if (canSkipped) yield return new WaitUntil(() => !cutscenePlayer.isPlaying || Input.GetKeyDown(Player.keybinds.startSongKeyCode));
        if (!canSkipped) yield return new WaitUntil(() => !cutscenePlayer.isPlaying);
        Song.instance.cutsceneIsPlaying = false;
        cutscenePlayer.gameObject.SetActive(false);
        rawImage.gameObject.SetActive(false);
        subtitleOfCutscene.gameObject.SetActive(false);
        play = false;
    }

    public Dictionary<int, string> SubtitleParser(string[] lines) {
        Dictionary<int, string> subtitleLines = new Dictionary<int, string>();
        foreach (string line in lines) {
            string[] splittedLine = line.Split(':');
            subtitleLines.Add(Convert.ToInt32(splittedLine[0]), splittedLine[1]);
        }
        return subtitleLines;
    }

    public Dictionary<TimeSpan, string> TimeStampCalculator(Dictionary<string, string> timeStamps, Dictionary<int, string> subtitlesContent) {
        List<string> timeStampNumbers = timeStamps.Keys.ToList();
        List<string> timeStampsInVideo = timeStamps.Values.ToList();
        List<int> keysOfContent = subtitlesContent.Keys.ToList();
        List<string> valuesOfContent = subtitlesContent.Values.ToList();
        List<TimeSpan> timeSpans = new List<TimeSpan>();
        Dictionary<TimeSpan, string> contentsParsed = new Dictionary<TimeSpan, string>();

        foreach (string rawTimeInVideo in timeStampsInVideo) {
            timeSpans.Add(TimeSpan.Parse(rawTimeInVideo));
        }

        foreach (TimeSpan span in timeSpans) {
            contentsParsed.Add(span, valuesOfContent[timeSpans.IndexOf(span)]);
        }

        return contentsParsed;
    }
}


public class CutsceneDataInternal {
    [JsonProperty("Cutscene")] public CutsceneData cutscene = new CutsceneData();
}

public class CutsceneData {
    [JsonProperty("Internal Name")] public string InternalName = "";
    [JsonProperty("Cutscene File Name")] public string CutsceneFileName = "";
    [JsonProperty("Has Subtitles")] public bool HasSubtitles = false;
    [JsonProperty("Can Jumped")] public bool CanJumped = true;
    [JsonProperty("Subtitles File Name")] public Dictionary<string, string> SubtitlesFileName = new Dictionary<string, string>();
    [JsonProperty("Time Stamps")] public Dictionary<string, string> TimeStamps = new Dictionary<string, string>();
}

public class FontDataInternal {
    [JsonProperty("Font")] public FontData font = new FontData();
}

public class FontData {
    [JsonProperty("File Name")] public string fileName = "";
    [JsonProperty("Size")] public int size = 24;
}
