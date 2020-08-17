using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//The PlayerUI manage the player HUD during the matches.
public class PlayerUI : MonoBehaviour
{
    public GameObject    player;
    public GameManager   gameManager;

    [Header("PlayerHUD")]
    public Slider        healthBar;
    public Slider        armorBar;
    public Text          ammoText;
    public Text          medKitText;
    public Text          roundCounterText;

    [Header("Screens")]
    public GameObject pauseMenu;
    public GameObject gameOverScreen;

    private bool _pauseMenuActive = false;

    private PlayerSystem     _playerSystem;
    private PlayerInput      _playerInput;
    private CameraController _cameraController;



    private void Start()
    {
        _playerSystem     = player.GetComponent<PlayerSystem>();
        _playerInput      = player.GetComponent<PlayerInput>();
        _cameraController = player.GetComponentInChildren<CameraController>();

        LockCursor();

        Time.timeScale = 1;
    }

    //____________Cursor_____________\\

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    //____________Updates_____________\\

    private void Update()
    {
        UpdateUIValues();
        GameOver();
        ActivateDeactivatePauseMenu();
    }

    private void UpdateUIValues()
    {
        healthBar.value = _playerSystem._healthPoints;
        armorBar.value  = _playerSystem._armorPoints;

        ammoText.text   = "Ammo = " + _playerSystem._magazine;
        medKitText.text = "Medkits = " + _playerSystem._medKitsUsage;

        roundCounterText.text = "Round - " + gameManager._currentRound;
    }


    private void ActivateDeactivatePauseMenu()
    {
        if (_playerInput.pause && _playerSystem._healthPoints > 0)
        {
            if (!_pauseMenuActive)
            {
                _pauseMenuActive                  = true;
                _cameraController._isCameraLocked = true;

                Time.timeScale = 0;
                pauseMenu.SetActive(true);

                UnlockCursor();
            }
            else
            {
                _pauseMenuActive                  = false;
                _cameraController._isCameraLocked = false;

                Time.timeScale = 1;
                pauseMenu.SetActive(false);

                LockCursor();
            }
        }
    }

    private void GameOver()
    {
        if (_playerSystem._healthPoints <= 0)
        {
            gameOverScreen.SetActive (true);
            Time.timeScale = 0;

            UnlockCursor();

            _cameraController._isCameraLocked = true;
        }
    }

    //____________PauseMenuButtons_____________\\

    public void OnReturnButtonClick()
    {
        _pauseMenuActive = false;
        _cameraController._isCameraLocked = false;

        Time.timeScale = 1;
        pauseMenu.SetActive(false);

        LockCursor();
    }

    public void OnRestartButtonClick() 
    {
        SceneManager.LoadScene("Level_1");
        _cameraController._isCameraLocked = true;
    }

    public void OnQuitToMenuButtonClick()
    {
        SceneManager.LoadScene("MainMenu");
        _cameraController._isCameraLocked = false;
    }

}
