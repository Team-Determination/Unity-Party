using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FridayNightFunkin;

public class ChartParser : MonoBehaviour
{
    public FNFSong song;
    public GameObject parent;
    public GameObject sectionPrefab;
    public RaycastHit2D raycastUpHit, raycastDownHit;
    private void Start() {
        //LoadingTransition.instance.Hide();
        
        song = new FNFSong("C:/Users/Raony Reis/AppData/LocalLow/Rei/FridayNight/Bundles/vs_qt.1/01 Censory-Overload/Chart-hard.json");
        ParseJson();
        
    }

    private void Update() {
        raycastUpHit = Physics2D.Raycast(new Vector2(-5.27f, 5.68f), Vector2.zero, float.MaxValue);
        raycastDownHit = Physics2D.Raycast(new Vector2(-5.21f, -5.56f), Vector2.zero, float.MaxValue);

        if (raycastUpHit.collider != null) {
            print("Up");
        }

        if (raycastDownHit.collider != null) {
            print("Down");
        }
    }

    void ParseJson() {
        foreach (FNFSong.FNFSection section in song.Sections) {
            int height = 0;
            GameObject sectionObject = Instantiate(sectionPrefab, parent.transform);
            sectionObject.name = "Section: " + song.Sections.IndexOf(section);
            Section sectionMeta = sectionObject.GetComponent<Section>();
            foreach (FNFSong.FNFNote note in section.Notes) {
                height += (note.Time % 1000 == 0) ? 1 : 0;
                foreach (Minisection minisection in sectionMeta.minisections) {
                    foreach (GameObject noteObject in minisection.notesInSection)
                        noteObject.SetActive(false);

                    minisection.SetActive(section.Notes.IndexOf(note), true);
                }
            }
        }
    }

    public void RegisterInChanges(Section section) {
        
    }
}