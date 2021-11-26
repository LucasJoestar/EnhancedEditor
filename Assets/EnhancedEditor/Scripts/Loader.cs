using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loader : MonoBehaviour
{
    public EnhancedEditor.BuildSceneDatabase Database;
    public EnhancedEditor.SceneBundle bundle;
    public Text m_Text;
    public Button m_Button;

    public int LoadSceneIndex = 0;

    bool load = true;

    void Start()
    {
        EnhancedEditor.BuildSceneDatabase.Database = Database;
        m_Button.onClick.AddListener(LoadButton);
        DontDestroyOnLoad(this);
    }

    void LoadButton()
    {
        if (load)
        {
            StartCoroutine(LoadScene());
        }
        else
        {
            StartCoroutine(Unload());
        }
    }

    IEnumerator LoadScene()
    {
        m_Text.text = "Lading";

        yield return null;
        yield return bundle.LoadAsync(LoadSceneMode.Additive);

        m_Text.text = "Loaded Successfully";
        load = false;
    }

    IEnumerator Unload()
    {
        m_Text.text = "Unloading";

        yield return null;
        yield return bundle.UnloadAsync();

        m_Text.text = "Unloaded Successfully";
        load = true;
    }
}