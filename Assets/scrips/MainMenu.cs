using UnityEngine;
using UnityEngine.SceneManagement; // 切換場景需要用到

public class MainMenu : MonoBehaviour
{
    // 用來存放你的 Option 視窗物件
    public GameObject option_window;

    // 模擬音量設定的變數（示範用，你可以根據之後的 UI 調整）
    private float volume_setting = 1.0f;

    private void Start(){
        // 遊戲一開始，自動讀取之前存過的音量設定（如果沒有紀錄過，預設值給 1.0）
        volume_setting = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        Debug.Log("已讀取音量設定：" + volume_setting);
        
        // 在這裡把讀取到的數值，套用到音量控制元件上
    }

    private void Update(){
        // 偵測玩家是否按Esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 檢查目前option_window是否是打開的 (Active)
            if (option_window != null && option_window.activeSelf)
            {
                close_option(); // 如果是開著的，就關閉它
            }
        }
    }

    // 1. 開始遊戲
    public void start_game(){
        SceneManager.LoadScene("main_game"); 
    }

    // 2. 打開設定視窗
    public void open_option(){
        if (option_window != null)
        {
            option_window.SetActive(true); // 顯示設定視窗
        }
    }

    // 3. 關閉設定視窗
    public void close_option(){
        if (option_window != null)
        {
            //在關閉視窗時把當前的設定存起來
            save_settings();

            option_window.SetActive(false); // 隱藏設定視窗
        }
    }

    // 4. 離開遊戲
    public void exit_game()
    {
        //在離開遊戲時再儲存一次
        save_settings();

        Debug.Log("遊戲已關閉並已儲存資料"); 
        Application.Quit();     
    }

    // 5. 專門處理儲存的函數
    private void save_settings()
    {
        //假設玩家調整了音量(先用變數做示範)
        //儲存語法：PlayerPrefs.SetFloat("鑰匙名稱", 要存的數值);
        PlayerPrefs.SetFloat("MusicVolume", volume_setting);

        //將資料儲存
        PlayerPrefs.Save();
        Debug.Log("設定資料已成功儲存到硬碟");
    }
}