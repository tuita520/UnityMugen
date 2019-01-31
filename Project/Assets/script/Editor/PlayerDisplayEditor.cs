﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Mugen;

public struct SelctedItem
{
	public SelctedItem(int stateIdx = -1, int palIdx = -1, bool clsn = false, int commandIdx = -1)
    {
        stateIndex = stateIdx;
        palletIndex = palIdx;
		showClsn = clsn;
        commandIndex = commandIdx;
    }
    public int stateIndex ;
    public int palletIndex;
	public bool showClsn;
    public int commandIndex;
}

[CustomEditor(typeof(PlayerDisplay))]
public class PlayerDisplayEditor : Editor {
    private List<PlayerState> m_VaildStateList = null;
    private string[] m_VaildPalletNameList = null;
    private string[] m_VaildStateNameList = null;
    private Cmd_Command[] m_CommandList = null;
   // private string[] m_CommandNameList = null;
    private PlayerDisplay m_LastDisplay = null;
    private int m_StateSelected = -1;
    private int m_PalletSelectd = -1;
	private bool m_ShowClsn = false;
    private static Dictionary<int, SelctedItem> m_SelectedMap = new Dictionary<int, SelctedItem>();
    private int m_AniSelect = -1;
    private int m_CommandSel = -1;

    private void InitPlayerDisplay()
    {
        m_VaildStateList = null;
        m_VaildStateNameList = null;
        m_VaildPalletNameList = null;
        m_StateSelected = -1;
        m_PalletSelectd = -1;
        m_AniSelect = -1;
		m_ShowClsn = false;
        m_CommandList = null;
       // m_CommandNameList = null;
        m_CommandSel = -1;
        if (m_LastDisplay == null)
            return;
        var player = m_LastDisplay.GPlayer;
        if (player == null)
        {
            // 可能还没有加载完数据
            m_LastDisplay = null;
            return;
        }
        if (player.AirCfg != null && player.AirCfg.IsVaild)
        {
            for (int i = 0; i < player.AirCfg.GetStateCount(); ++i)
            {
                var key = player.AirCfg.GetStateByIndex(i);
                if (m_VaildStateList == null)
                    m_VaildStateList = new List<PlayerState>();
                m_VaildStateList.Add(key);
            }

            // 刪除無效狀態
            for (int i = m_VaildStateList.Count - 1; i >= 0; --i)
            {
                var state = m_VaildStateList[i];
                if (!m_LastDisplay.HasBeginActionSrpiteData(state, true))
                {
                   // Debug.LogErrorFormat("BeginAction {0:D} is not vaild~!", (int)state);
                    m_VaildStateList.RemoveAt(i);
                }
            }
            //--------------------------

            if (m_VaildStateList != null && m_VaildStateList.Count > 0)
            {
                m_VaildStateNameList = new string[m_VaildStateList.Count];
                for (int i = 0; i < m_VaildStateList.Count; ++i)
                {
                    var state = m_VaildStateList[i];
                    m_VaildStateNameList[i] = string.Format("{0}({1:D})", state.ToString(), (int)state);
                }
            }

            SelctedItem selItem;
			if (m_SelectedMap.TryGetValue (m_LastDisplay.GetInstanceID (), out selItem)) {
				var state = m_VaildStateList [selItem.stateIndex];
				if (m_LastDisplay.PlayAni (state, true))
					m_StateSelected = selItem.stateIndex;
			} else {
				var ani1 = m_LastDisplay.ImageAni;
				if (ani1 != null) {
					if (ani1.State != PlayerState.psNone) {
						for (int i = 0; i < m_VaildStateList.Count; ++i) {
							if (ani1.State == m_VaildStateList [i]) {
								m_StateSelected = i;
								break;
							}
						}
					}
				}
			}
        }

        var loaderPlayer = m_LastDisplay.LoaderPlayer;
        if (loaderPlayer != null && loaderPlayer.PalNameList != null)
        {
            m_VaildPalletNameList = loaderPlayer.PalNameList.ToArray();
            SelctedItem selItem;
            if (m_SelectedMap.TryGetValue(m_LastDisplay.GetInstanceID(), out selItem))
            {
                m_PalletSelectd = selItem.palletIndex;
            }
        }

		SelctedItem selItem1;
		if (m_SelectedMap.TryGetValue (m_LastDisplay.GetInstanceID (), out selItem1)) {
			m_ShowClsn = selItem1.showClsn;
		}

        m_AniSelect = m_LastDisplay.ImageAni.CurFrame;

        if (player.CmdCfg != null)
        {
            m_CommandList = player.CmdCfg.GetCommandArray();
            SelctedItem selItem;
            if (m_SelectedMap.TryGetValue(m_LastDisplay.GetInstanceID(), out selItem))
            {
                m_CommandSel = selItem.commandIndex;
            }

            /*
            if (m_CommandList != null && m_CommandList.Length > 0)
            {
                m_CommandNameList = new string[m_CommandList.Length];
                for (int i = 0; i < m_CommandList.Length; ++i)
                {
                    m_CommandNameList[i] = m_CommandList[i].name;
                }
            }
             * */
        }
    }

    private void DrawPlayerDisplay()
    {
        if (m_LastDisplay == null)
            return;
        if (m_VaildStateList != null && m_VaildStateNameList != null)
        {
            EditorGUILayout.BeginVertical();
            string l0 = string.Format("手动选择角色动作({0:D})", m_VaildStateNameList.Length);
            int newSelected = EditorGUILayout.Popup(l0, m_StateSelected, m_VaildStateNameList);
            if (m_StateSelected != newSelected)
            {
                var state = m_VaildStateList[newSelected];
                m_StateSelected = newSelected;
                if (m_LastDisplay.PlayAni(state, true))
                {
                    SelctedItem item;
                    if (!m_SelectedMap.TryGetValue(m_LastDisplay.GetInstanceID(), out item))
                        item = new SelctedItem();
                    item.stateIndex = m_StateSelected;
                    m_SelectedMap[m_LastDisplay.GetInstanceID()] = item;
                }
            }

            if (m_VaildStateNameList != null && m_VaildStateNameList.Length > 0)
            {
                if (m_StateSelected >= 0)
                {
					GUILayout.BeginHorizontal ();
					if (GUILayout.Button ("上一个动作")) {
						int newState = m_StateSelected;
						if (newState - 1 < 0)
							newState = m_VaildStateNameList.Length - 1;
						else
							--newState;
						var state = m_VaildStateList[newState];
						if (state != PlayerState.psNone)
						{
							if (m_LastDisplay.PlayAni(state, true))
							{
								m_StateSelected = newState;

								SelctedItem item;
								if (!m_SelectedMap.TryGetValue(m_LastDisplay.GetInstanceID(), out item))
									item = new SelctedItem();
								item.stateIndex = m_StateSelected;
								m_SelectedMap[m_LastDisplay.GetInstanceID()] = item;
							}
						}
					}

                    if (GUILayout.Button("下一个动作"))
                    {
                        int newState = m_StateSelected;
                        if (newState + 1 >= m_VaildStateNameList.Length)
                            newState = 0;
                        else
                            ++newState;
                        var state = m_VaildStateList[newState];
                        if (state != PlayerState.psNone)
                        {
                            if (m_LastDisplay.PlayAni(state, true))
                            {
                                m_StateSelected = newState;

                                SelctedItem item;
                                if (!m_SelectedMap.TryGetValue(m_LastDisplay.GetInstanceID(), out item))
                                    item = new SelctedItem();
                                item.stateIndex = m_StateSelected;
                                m_SelectedMap[m_LastDisplay.GetInstanceID()] = item;
                            }
                        }
                    }
					GUILayout.EndHorizontal ();
                }
            }

            EditorGUILayout.EndVertical();
        }

        if (m_VaildPalletNameList != null)
        {
            string l1 = string.Format("手动选择调色板({0:D})", m_VaildPalletNameList.Length);
            int newSelected = EditorGUILayout.Popup(l1, m_PalletSelectd, m_VaildPalletNameList);
            if (m_PalletSelectd != newSelected)
            {
                string newPalletName = m_VaildPalletNameList[newSelected];
                m_PalletSelectd = newSelected;
                
                SelctedItem item;
                if (!m_SelectedMap.TryGetValue(m_LastDisplay.GetInstanceID(), out item))
                    item = new SelctedItem();
                item.palletIndex = m_PalletSelectd;
                m_SelectedMap[m_LastDisplay.GetInstanceID()] = item;

                m_LastDisplay.PalletName = newPalletName;
            }

			if (m_VaildPalletNameList.Length > 0 && m_PalletSelectd >= 0) {
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("上一个调色版")) {
					int newIdx = m_PalletSelectd - 1;
					if (newIdx < 0)
						newIdx = m_VaildPalletNameList.Length - 1;
					string ps = m_VaildPalletNameList [newIdx];
					if (!string.IsNullOrEmpty (ps)) {
						m_PalletSelectd = newIdx;
						SelctedItem item;
						if (!m_SelectedMap.TryGetValue(m_LastDisplay.GetInstanceID(), out item))
							item = new SelctedItem();
						item.palletIndex = m_PalletSelectd;
						m_SelectedMap[m_LastDisplay.GetInstanceID()] = item;
						m_LastDisplay.PalletName = ps;
					}
				}
				if (GUILayout.Button ("下一个调色版")) {
					int newIdx = m_PalletSelectd + 1;
					if (newIdx >= m_VaildPalletNameList.Length)
						newIdx = 0;
					string ps = m_VaildPalletNameList [newIdx];
					if (!string.IsNullOrEmpty (ps)) {
						m_PalletSelectd = newIdx;
						SelctedItem item;
						if (!m_SelectedMap.TryGetValue(m_LastDisplay.GetInstanceID(), out item))
							item = new SelctedItem();
						item.palletIndex = m_PalletSelectd;
						m_SelectedMap[m_LastDisplay.GetInstanceID()] = item;
						m_LastDisplay.PalletName = ps;
					}
				}
				GUILayout.EndHorizontal ();
			}
        }

		bool newShowClsn = EditorGUILayout.Toggle ("显示包围盒", m_ShowClsn);
		if (m_ShowClsn != newShowClsn) {
			m_ShowClsn = newShowClsn;
			SelctedItem item;
			if (!m_SelectedMap.TryGetValue(m_LastDisplay.GetInstanceID(), out item))
				item = new SelctedItem();
			item.showClsn = newShowClsn;
			m_SelectedMap[m_LastDisplay.GetInstanceID()] = item;
			ShowClsn (newShowClsn);
		}

		if (GUILayout.Button ("动画重置")) {
			m_LastDisplay.ResetFirstFrame ();
            m_AniSelect = 0;
		}

        var imgAni = m_LastDisplay.ImageAni;
        if (imgAni != null)
        {
            int aniCount = imgAni.AniNodeCount;
            if (aniCount > 0)
            {
                var ani = imgAni.CacheAnimation;
                if (ani != null)
                {
                  //  float c = GUILayout.HorizontalSlider(m_AniSelect, -1, aniCount);
                    int c = EditorGUILayout.IntSlider(m_AniSelect, 0, aniCount);
                    if (m_AniSelect != c)
                    {
                        m_AniSelect = c;
                        int cc = (int)c;
                        if (cc != imgAni.CurFrame)
                        {
                            imgAni.SetManualFrame(cc);
                        }
                    }
                }
            }
        }

		int curFrame = m_LastDisplay.ImageCurrentFrame;
        string s;
        if (imgAni == null)
            s = "(无)";
        else
        {
            var aniNode = imgAni.CurAniNode;
            if (aniNode.frameIndex < 0)
                s = curFrame.ToString();
            else
                s = string.Format("{0:D}({1:D}:{2:D})", curFrame, aniNode.frameGroup, aniNode.frameIndex);
        }
		EditorGUILayout.LabelField ("动画当前帧", s);
        string palletName = m_LastDisplay.PalletName;
        string str = palletName;
        if (string.IsNullOrEmpty(str))
            str = "(无)";
        EditorGUILayout.LabelField("当前调色板", str);
        if (!string.IsNullOrEmpty(palletName) && m_PalletSelectd < 0)
        {
            for (int i = 0; i < m_VaildPalletNameList.Length; ++i)
            {
                if (string.Compare(palletName, m_VaildPalletNameList[i], true) == 0)
                {
                    m_PalletSelectd = i;
                    break;
                }
            }
        }

		int playerType = (int)m_LastDisplay.PlyType;
		EditorGUILayout.LabelField("角色控制", playerType.ToString());

        // 显示命令
        if (m_CommandList != null && m_CommandList.Length > 0 /*&& m_CommandNameList != null && m_CommandNameList.Length > 0*/)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("【操作命令】");
            //GUILayout.BeginArea(new Rect(0, Screen, Screen.width, 400));
            int newCmdIdx = m_CommandSel;

            int idx = 0;
            bool isVV = false;
            for (int i = 0; i < m_CommandList.Length; ++i)
            {
                var cmd = m_CommandList[i];
                if (cmd == null)
                    continue;
                
                if (idx % 2 == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    isVV = true;
                }
                if (GUILayout.Button(cmd.name))
                {
                    newCmdIdx = i;
                }

                if (idx % 2 == 1 && isVV)
                {
                    EditorGUILayout.EndHorizontal();
                    isVV = false;
                }

                ++idx;
            }

            if (isVV)
                EditorGUILayout.EndHorizontal();

            // GUILayout.EndArea();
            if (m_CommandSel != newCmdIdx)
            {
                m_CommandSel = newCmdIdx;

                SelctedItem item;
                if (!m_SelectedMap.TryGetValue(m_LastDisplay.GetInstanceID(), out item))
                    item = new SelctedItem();
                item.commandIndex = newCmdIdx;
                m_SelectedMap[m_LastDisplay.GetInstanceID()] = item;

                // 执行命令
                string cmdName = m_CommandList[m_CommandSel].name;
                m_LastDisplay.RunCmd(cmdName);
            }
        }
    }

	void ShowClsn(bool isShow)
	{
		var target = this.target as PlayerDisplay;
		if (target == null)
			return;
		target.ShowClsn (isShow);
	}

	public override void OnInspectorGUI ()
    {
        DrawDefaultInspector();

        if (Application.isPlaying)
        {
            if (m_LastDisplay != this.target)
            {
                m_LastDisplay = this.target as PlayerDisplay;
                InitPlayerDisplay();
            }

            DrawPlayerDisplay();

        } else
        {
            if (m_SelectedMap.Count > 0)
                m_SelectedMap.Clear();
        }

    }

    [MenuItem("Assets/Mugen/Mugen资源转Unity资源", true)]
    [MenuItem("Assets/Mugen/Unity资源转Mugen资源", true)]
    public static bool IsProcessMugenFilesToUnityResFiles()
    {
        return Selection.activeGameObject == null && Selection.activeObject != null ;
    }

    // 选中文件夹处理
    [MenuItem("Assets/Mugen/Mugen资源转Unity资源")]
    public static void ProcessMugenFilesToUnityResFiles()
    {
        var selObj = Selection.activeObject;
        if (selObj != null)
        {
            string path = AssetDatabase.GetAssetPath(selObj);
            if (string.IsNullOrEmpty(path))
                return;
            path = System.IO.Path.GetDirectoryName(path);
            string[] files = System.IO.Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories);
            if (files != null && files.Length > 0)
            {
                for (int i = 0; i < files.Length; ++i)
                {
                    string srcPath = files[i];
                    if (string.IsNullOrEmpty(srcPath))
                        continue;
                    string ext = System.IO.Path.GetExtension(srcPath);
                    if (string.IsNullOrEmpty(ext))
                        continue;
                    string chgExt = string.Empty;
                    if (string.Compare(ext, ".def", true) == 0)
                        chgExt = ".def.txt";
                    else if (string.Compare(ext, ".sff", true) == 0)
                        chgExt = ".sff.bytes";
                    else if (string.Compare(ext, ".snd", true) == 0)
                        chgExt = ".snd.bytes";
                    else if (string.Compare(ext, ".air", true) == 0)
                        chgExt = ".air.txt";
                    else if (string.Compare(ext, ".act", true) == 0)
                        chgExt = ".act.bytes";
                    else if (string.Compare(ext, ".ai", true) == 0)
                        chgExt = ".ai.bytes";
                    else if (string.Compare(ext, ".cmd", true) == 0)
                        chgExt = ".cmd.txt";
                    else if (string.Compare(ext, ".cns", true) == 0)
                        chgExt = ".cns.txt";
                    if (string.IsNullOrEmpty(chgExt))
                        continue;
                    string dstPath = System.IO.Path.ChangeExtension(srcPath, chgExt);
                  //  FileUtil.ReplaceFile(srcPath, dstPath);
                    System.IO.File.Move(srcPath, dstPath);
                }

                AssetDatabase.Refresh();
            }
        }
    }

    [MenuItem("Assets/Mugen/Unity资源转Mugen资源")]
    // 选中文件夹处理
    public static void ProcessUnityResFilesToMugenFiles()
    {
        var selObj = Selection.activeObject;
        if (selObj != null)
        {
            string path = AssetDatabase.GetAssetPath(selObj);
            if (string.IsNullOrEmpty(path))
                return;
            path = System.IO.Path.GetDirectoryName(path);
            string[] files = System.IO.Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories);
            if (files != null && files.Length > 0)
            {
                for (int i = 0; i < files.Length; ++i)
                {
                    string srcPath = files[i];
                    if (string.IsNullOrEmpty(srcPath))
                        continue;
                    string ext = System.IO.Path.GetExtension(srcPath);
                    if (string.IsNullOrEmpty(ext))
                        continue;
                    string chgExt = string.Empty;
                    if (ext.IndexOf(".def.txt", System.StringComparison.CurrentCultureIgnoreCase) >= 0)
                        chgExt = ext.Replace(".def.txt", ".def");
                    else if (ext.IndexOf(".sff.bytes", System.StringComparison.CurrentCultureIgnoreCase) >= 0)
                        chgExt = ext.Replace(".sff.bytes", ".sff");
                    else if (ext.IndexOf(".snd.bytes", System.StringComparison.CurrentCultureIgnoreCase) >= 0)
                        chgExt = ext.Replace(".snd.bytes", ".snd");
                    else if (ext.IndexOf(".air.txt", System.StringComparison.CurrentCultureIgnoreCase) >= 0)
                        chgExt = ext.Replace(".air.txt", ".air");
                    else if (ext.IndexOf(".act.bytes", System.StringComparison.CurrentCultureIgnoreCase) >= 0)
                        chgExt = ext.Replace(".act.bytes", ".act");
                    else if (ext.IndexOf(".ai.bytes", System.StringComparison.CurrentCultureIgnoreCase) >= 0)
                        chgExt = ext.Replace(".ai.bytes", ".ai");
                    else if (ext.IndexOf(".cmd.txt", System.StringComparison.CurrentCultureIgnoreCase) >= 0)
                        chgExt = ext.Replace(".cmd.txt", ".cmd");
                    else if (ext.IndexOf(".cns.txt", System.StringComparison.CurrentCultureIgnoreCase) >= 0)
                        chgExt = ext.Replace(".cns.txt", ".cns");

                    if (string.IsNullOrEmpty(chgExt))
                        continue;
                    string dstPath = System.IO.Path.ChangeExtension(srcPath, chgExt);
                    System.IO.File.Move(srcPath, dstPath);
                }

                AssetDatabase.Refresh();
            }

        }
    }
}
