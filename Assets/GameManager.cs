﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using System.IO;

public class GameManager : MonoBehaviour
{
    public delegate bool LoadCoroutine();

    private static GameManager _Instance = null;
    public static GameManager Instance
    {
        get
        {
            if (_Instance == null) _Instance = FindObjectOfType<GameManager>();
            return _Instance;
        }
    }

    private static bool CheckServerConfig()
    {
        if (ResourceManager.FileExists("server.cfg"))
        {
            StringFile sf = new StringFile("server.cfg");
            // execute all commands from there
            foreach (string cmd in sf.Strings)
            {
                Debug.Log(cmd);
                GameConsole.Instance.ExecuteCommand(cmd);
            }
            return true;
        }

        return false;
    }

    // since this is a part of global state
    private bool _IsHeadlessChecked = false;
    private bool _IsHeadless = false;
    public bool IsHeadless
    {
        get
        {
            if (!_IsHeadlessChecked)
            {
                string[] args = Environment.GetCommandLineArgs();
                if (args.Contains("-nographics") || args.Contains("-batchmode"))
                    _IsHeadless = true;
                _IsHeadlessChecked = true;
            }

            return _IsHeadless;
        }
    }

    public MapView MapView;
    public GameConsole GameConsole;

    void Awake()
    {
        CheckServerConfig();
        Locale.InitLocale(); // load locale strings, like main.txt, patch.txt, etc
    }

    void Start()
    {
        GameConsole = Utils.CreateObjectWithScript<GameConsole>();
        GameConsole.transform.parent = UiManager.Instance.transform;
        Config.Load();
    }

    void OnDestroy()
    {

    }

    private IEnumerator DelegateCoroutine(LoadCoroutine del)
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (!del())
                break;
        }
    }

    public void CallDelegateOnNextFrame(LoadCoroutine del)
    {
        StartCoroutine(DelegateCoroutine(del));
    }

    void Update()
    {

    }
}
