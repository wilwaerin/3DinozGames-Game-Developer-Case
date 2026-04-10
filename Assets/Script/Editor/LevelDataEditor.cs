using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    private RingColor selectedColor = RingColor.Red;
    private const int CellSize = 30;
    private const int MaxSlotsPerRow = 8;

    private static readonly RingColor[] AllColors =
    {
        RingColor.Red, RingColor.Blue, RingColor.Green,
        RingColor.Yellow, RingColor.Orange, RingColor.Pink
    };

    public override void OnInspectorGUI()
    {
        LevelData data = (LevelData)target;
        serializedObject.Update();

        EditorGUILayout.Space(4);

        // ── TIMER ──────────────────────────────────────────
        DrawSectionHeader("  Timer", new Color(0.35f, 0.78f, 0.55f));
        data.levelTime = EditorGUILayout.FloatField("Level Time (seconds)", data.levelTime);
        if (data.levelTime < 1f) data.levelTime = 1f;
        EditorGUILayout.Space(20);

        // ── COLOR PALETTE ──────────────────────────────────
        DrawSectionHeader("  Color Palette  —  click to select", new Color(0.55f, 0.55f, 0.55f));
        DrawColorPalette();
        EditorGUILayout.Space(20);

        // ── CHAINS ─────────────────────────────────────────
        DrawSectionHeader($"  Chains  ({data.chains.Count} / {LevelData.MaxChains})  left → right", new Color(0.3f, 0.58f, 1f));
        EditorGUILayout.Space(2);
        DrawMatrix(data, isChain: true);
        EditorGUILayout.Space(20);

        // ── STICK COLUMNS ──────────────────────────────────
        DrawSectionHeader($"  Stick Columns  ({data.stickColumns.Count} / {LevelData.MaxColumns})   left → right", new Color(1f, 0.72f, 0.18f));
        EditorGUILayout.Space(2);
        DrawMatrix(data, isChain: false);

        EditorGUILayout.Space(4);
        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
            EditorUtility.SetDirty(data);
    }

    // ── Color Palette ──────────────────────────────────────────────────────────
    private void DrawColorPalette()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(6);

        foreach (RingColor rc in AllColors)
        {
            bool active = selectedColor == rc;
            GUI.backgroundColor = active ? Color.white : new Color(0.75f, 0.75f, 0.75f);

            GUIStyle style = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                fontSize  = active ? 13 : 10,
                alignment = TextAnchor.MiddleCenter
            };

            Color prev = GUI.color;
            GUI.color = GetEditorColor(rc);
            string label = active ? "●" : "○";
            if (GUILayout.Button(label, style, GUILayout.Width(CellSize + 10), GUILayout.Height(CellSize + 6)))
                selectedColor = rc;
            GUI.color = prev;
        }

        GUI.backgroundColor = Color.white;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(3);
        GUIStyle nameStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold
        };
        GUI.color = GetEditorColor(selectedColor);
        EditorGUILayout.LabelField($"  Selected:  {selectedColor}", nameStyle);
        GUI.color = Color.white;
        EditorGUILayout.Space(4);
    }

    // ── Generic Matrix — vertical columns ─────────────────────────────────────
    private void DrawMatrix(LevelData data, bool isChain)
    {
        int count    = isChain ? data.chains.Count  : data.stickColumns.Count;
        int maxCount = isChain ? LevelData.MaxChains : LevelData.MaxColumns;
        string colLbl = isChain ? "C" : "S";

        int toRemove = -1;

        // Find tallest column for padding
        int maxSlots = 0;
        for (int i = 0; i < count; i++)
        {
            int c = isChain ? data.chains[i].ringColors.Count : data.stickColumns[i].stickColors.Count;
            if (c > maxSlots) maxSlots = c;
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(6);

        for (int i = 0; i < count; i++)
        {
            List<RingColor> colors = isChain
                ? data.chains[i].ringColors
                : data.stickColumns[i].stickColors;

            EditorGUILayout.BeginVertical(GUILayout.Width(CellSize + 2));

            // Column label
            GUIStyle lbl = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                { fontSize = 10, fontStyle = FontStyle.Bold };
            EditorGUILayout.LabelField($"{colLbl}{i + 1}", lbl,
                GUILayout.Width(CellSize), GUILayout.Height(15));

            // Color cells — top to bottom
            for (int j = 0; j < colors.Count; j++)
            {
                GUI.backgroundColor = GetEditorColor(colors[j]);
                if (GUILayout.Button("", GUILayout.Width(CellSize), GUILayout.Height(CellSize)))
                {
                    Undo.RecordObject(data, "Paint Color");
                    colors[j] = selectedColor;
                    EditorUtility.SetDirty(data);
                }
            }
            GUI.backgroundColor = Color.white;

            // Bottom padding so control buttons stay at consistent height
            int pad = maxSlots - colors.Count;
            if (pad > 0) GUILayout.Space(pad * (CellSize + 2));

            EditorGUILayout.Space(3);

            // + add slot
            if (colors.Count < MaxSlotsPerRow)
            {
                GUI.backgroundColor = new Color(0.55f, 0.55f, 0.55f);
                if (GUILayout.Button("+", GUILayout.Width(CellSize), GUILayout.Height(20)))
                {
                    Undo.RecordObject(data, "Add Slot");
                    colors.Add(selectedColor);
                    EditorUtility.SetDirty(data);
                }
                GUI.backgroundColor = Color.white;
            }

            // – remove last slot
            if (colors.Count > 0)
            {
                GUI.backgroundColor = new Color(0.85f, 0.4f, 0.4f);
                if (GUILayout.Button("-", GUILayout.Width(CellSize), GUILayout.Height(20)))
                {
                    Undo.RecordObject(data, "Remove Slot");
                    colors.RemoveAt(colors.Count - 1);
                    EditorUtility.SetDirty(data);
                }
                GUI.backgroundColor = Color.white;
            }

            // X remove column
            GUI.backgroundColor = new Color(1f, 0.28f, 0.28f);
            GUIStyle xStyle = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold };
            if (GUILayout.Button("X", xStyle, GUILayout.Width(CellSize), GUILayout.Height(20)))
                toRemove = i;
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();
            GUILayout.Space(3);
        }

        // Add new column button
        if (count < maxCount)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(CellSize + 2));
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = new Color(0.45f, 0.75f, 0.45f);
            GUIStyle addStyle = new GUIStyle(GUI.skin.button)
                { fontStyle = FontStyle.Bold, fontSize = 13, alignment = TextAnchor.MiddleCenter };
            string addLbl = isChain ? "+\nC" : "+\nS";
            if (GUILayout.Button(addLbl, addStyle, GUILayout.Width(CellSize), GUILayout.Height(44)))
            {
                string undoName = isChain ? "Add Chain" : "Add Column";
                Undo.RecordObject(data, undoName);
                if (isChain) data.chains.Add(new ChainData());
                else         data.stickColumns.Add(new StickColumnData());
                EditorUtility.SetDirty(data);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (toRemove >= 0)
        {
            string undoName = isChain ? "Remove Chain" : "Remove Column";
            Undo.RecordObject(data, undoName);
            if (isChain) data.chains.RemoveAt(toRemove);
            else         data.stickColumns.RemoveAt(toRemove);
            EditorUtility.SetDirty(data);
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────
    private void DrawSectionHeader(string title, Color color)
    {
        GUI.backgroundColor = color;
        GUIStyle style = new GUIStyle(EditorStyles.toolbar)
        {
            fontStyle = FontStyle.Bold,
            fontSize  = 12
        };
        EditorGUILayout.LabelField(title, style, GUILayout.Height(22));
        GUI.backgroundColor = Color.white;
        EditorGUILayout.Space(2);
    }

    private Color GetEditorColor(RingColor ringColor)
    {
        switch (ringColor)
        {
            case RingColor.Red:    return new Color(1f, 0.35f, 0.35f);
            case RingColor.Blue:   return new Color(0.35f, 0.55f, 1f);
            case RingColor.Green:  return new Color(0.3f, 0.85f, 0.3f);
            case RingColor.Yellow: return new Color(1f, 0.92f, 0.2f);
            case RingColor.Orange: return new Color(1f, 0.6f, 0.1f);
            case RingColor.Pink:   return new Color(1f, 0.5f, 0.85f);
            default:               return Color.gray;
        }
    }
}
