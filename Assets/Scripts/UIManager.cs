using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameChannelSO gameChannel;
    [SerializeField] LocalChannelSO localChannel;
    [SerializeField] Slider localSlider;
    [SerializeField] TMP_Text localText;
    [SerializeField] Slider enemySlider;
    [SerializeField] TMP_Text enemyText;

    [SerializeField] TMP_Text goldText;
    [SerializeField] GameObject activeGameUI;
    [SerializeField] GameObject endScreenUI;
    [SerializeField] TMP_Text endScreenText;

    void Start()
    {
        gameChannel.OnRpcSetLife += OnRpcSetLife;
        localChannel.OnDecreaseLife += DecreaseLife;
        localChannel.OnSetGold += SetGold;
        gameChannel.GameEndEvent += SetGameEndUI;
        gameChannel.GameStartEvent += GameStart;
        AudioListener.volume = 0.5f;
    }

    private void SetGameEndUI(bool isWinner)
    {
        activeGameUI.SetActive(false);
        endScreenUI.SetActive(true);
        endScreenText.text = isWinner ? "Winner" : "Loser";
    }

    void GameStart()
    {
        activeGameUI.SetActive(true);
        endScreenUI.SetActive(false);
        MonoBehaviour hud = GameObject.Find("NetworkManager").GetComponent<NetworkManagerHUDRTC>();
        hud.enabled = false;
    }

    public void BackToLobby()
    {
        SceneManager.LoadScene("Lobby");
    }

    void OnRpcSetLife(int life, bool isLocal)
    {
        Slider slider = isLocal ? localSlider : enemySlider;
        TMP_Text text = isLocal ? localText : enemyText;

        slider.value = life;
        // text.text = $"{life} / {slider.maxValue}";
        text.text = $"{life}";
    }

    void DecreaseLife(int lives)
    {
        gameChannel.CmdSetLife((int)localSlider.value - lives);
    }

    void SetGold(int gold)
    {
        goldText.text = $"{gold}";
    }

}
