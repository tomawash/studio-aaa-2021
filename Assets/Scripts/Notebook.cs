using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notebook : MonoBehaviour
{
    public static Notebook instance;
    private void Awake()
    {
        if (instance)
            Destroy(instance.gameObject);
        instance = this;
    }

    private List<NotebookEntry> entries;
    [SerializeField]
    private NotebookEntry entryPrefab;

    public void addEntry(NotebookEntrySO entry)
    {
        NotebookEntry newEntry = Instantiate(entryPrefab, transform);
        newEntry.title.text = entry.title;
        newEntry.shortDescription.text = entry.shortDescription;
        entries.Add(newEntry);
    }
}
