using System;
using UnityEngine;
using UnityEngine.Audio;
using StarterAssets;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Settings : MonoBehaviour
{
    public AudioMixer masterMixer;
    private MainMenu mainMenu;
    [SerializeField]private FirstPersonController player;
    private Cinemachine.CinemachineVirtualCamera cinemachine;
    private Volume volume;
    private MotionBlur motionBlur;
    private string _master = "Master";
    private string _music = "Music";
    private string _audio = "Audio";
    private string _fov = "Fov";
    private string _sensibility = "Sensibility";
    private string _fullScreen = "Fullscreen";
    private string _headBob = "Headbob";
    private string _blur = "Blur";
    static private float sensitivityStatic;
    [SerializeField]private bool hold = true;
    [SerializeField]private bool playerHold = true;

    private float defaultFov = 60;
    private float defaultSensibility = 1;
    private void Awake()
    {
        mainMenu = GetComponent<MainMenu>();
        if (GameObject.FindGameObjectWithTag("Player"))
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();
            cinemachine = GameObject.Find("PlayerFollowCamera").GetComponent<Cinemachine.CinemachineVirtualCamera>();
            volume = GameObject.Find("Post Processing").GetComponent<Volume>();
            volume.profile.TryGet<MotionBlur>(out motionBlur);
        }
        if (mainMenu != null)
        {
            hold = false;
            if (player != null)
                playerHold = false;
        }
    }
    private void Start()
    {
        if (!hold)
        {
            mainMenu.MasterUpdate(PlayerPrefs.GetFloat(_master));
            mainMenu.MasterSlider(PlayerPrefs.GetFloat(_master));
            mainMenu.MusicUpdate(PlayerPrefs.GetFloat(_music));
            mainMenu.MusicSlider(PlayerPrefs.GetFloat(_music));
            mainMenu.AudioUpdate(PlayerPrefs.GetFloat(_audio));
            mainMenu.AudioSlider(PlayerPrefs.GetFloat(_audio));
            mainMenu.FovUpdate(PlayerPrefs.GetFloat(_fov));
            mainMenu.FovSlider(PlayerPrefs.GetFloat(_fov));
            mainMenu.SensibilityUpdate(PlayerPrefs.GetFloat(_sensibility));
            mainMenu.SensibilitySlider(PlayerPrefs.GetFloat(_sensibility));
            mainMenu.FullscreenUpdate(Convert.ToBoolean(PlayerPrefs.GetInt(_fullScreen)));
            mainMenu.HeadBobUpdate(Convert.ToBoolean(PlayerPrefs.GetInt(_headBob)));
            mainMenu.BlurUpdate(Convert.ToBoolean(PlayerPrefs.GetInt(_blur)));
        }
        if (!playerHold)
        {
            player.RotationSpeed = PlayerPrefs.GetFloat(_sensibility, defaultSensibility);
            player.CameraBob = Convert.ToBoolean(PlayerPrefs.GetInt(_headBob, 1));
            motionBlur.active = Convert.ToBoolean(PlayerPrefs.GetInt(_blur, 1));
            cinemachine.m_Lens.FieldOfView = PlayerPrefs.GetFloat(_fov, 1);
        }
    }
    public void MasterVolume(float volume)
    {
        masterMixer.SetFloat("Master", volume);
        PlayerPrefs.SetFloat(_master, volume);
        if(!hold)
            mainMenu.MasterUpdate(volume);
    }
    public void MusicVolume(float volume)
    {
        masterMixer.SetFloat("Music", volume);
        PlayerPrefs.SetFloat(_music, volume);
        if(!hold)
            mainMenu.MusicUpdate(volume);
    }
    public void AudioVolume(float volume)
    {
        masterMixer.SetFloat("Audio", volume);
        PlayerPrefs.SetFloat(_audio, volume);
        if(!hold)
            mainMenu.AudioUpdate(volume);
    }
    public void ToggleFullScreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(_fullScreen, Convert.ToInt32(isFullscreen));
    }
    public void ToggleHeadBob(bool isHeadBob)
    {
        if(!playerHold)
            player.CameraBob = isHeadBob;
        PlayerPrefs.SetInt(_headBob, Convert.ToInt32(isHeadBob));
    }
    public void ToggleBlur(bool isBlur)
    {
        if(!playerHold)
            motionBlur.active = isBlur;
        PlayerPrefs.SetInt(_blur, Convert.ToInt32(isBlur));
    }
    public void Sensibility(float sensibility)
    {
        PlayerPrefs.SetFloat(_sensibility, sensibility);
        if(!playerHold)
            player.RotationSpeed = sensibility;
        if(!hold)
            mainMenu.SensibilityUpdate(sensibility);
    }
    public void Fov(float fov)
    {
        PlayerPrefs.SetFloat(_fov, fov);
        if(!playerHold)
            cinemachine.m_Lens.FieldOfView = fov;
        if(!hold)
            mainMenu.FovUpdate(fov);
    }

    public void LoadValues()
    {
        MasterVolume(PlayerPrefs.GetFloat(_master, 0));
        MusicVolume(PlayerPrefs.GetFloat(_music, 0));
        AudioVolume(PlayerPrefs.GetFloat(_audio, 0));
        Fov(PlayerPrefs.GetFloat(_fov, defaultFov));
        Sensibility(PlayerPrefs.GetFloat(_sensibility, defaultSensibility));
        ToggleFullScreen(Convert.ToBoolean(PlayerPrefs.GetInt(_fullScreen, 1)));
        ToggleHeadBob(Convert.ToBoolean(PlayerPrefs.GetInt(_headBob, 1)));
        ToggleBlur(Convert.ToBoolean(PlayerPrefs.GetInt(_blur, 1)));
    }
}
