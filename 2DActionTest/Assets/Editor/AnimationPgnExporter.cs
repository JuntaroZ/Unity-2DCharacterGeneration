using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class AnimationPngExporter : EditorWindow
{
    private Animator animator;
    private AnimationClip clip;
    private Camera renderCamera;

    private int width = 280;
    private int height = 668;

    private int frameCount = 1;

    // =========================================================
    // EditorPrefs Keys
    // =========================================================

    private const string PrefAnimator =
        "AnimationPngExporter.Animator";

    private const string PrefClip =
        "AnimationPngExporter.AnimationClip";

    private const string PrefCamera =
        "AnimationPngExporter.Camera";

    // =========================================================
    // Window
    // =========================================================

    [MenuItem("Tools/Animation PNG Exporter")]
    public static void Open()
    {
        GetWindow<AnimationPngExporter>(
            "PNG Exporter"
        );
    }

    // =========================================================
    // Windowが有効になったとき
    // =========================================================

    private void OnEnable()
    {
        LoadPreviousSettings();
    }

    // =========================================================
    // GUI
    // =========================================================

    private void OnGUI()
    {
        GUILayout.Label(
            "Animation → PNG Export",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space();

        // =====================================================
        // Animator
        // =====================================================

        EditorGUI.BeginChangeCheck();

        Animator newAnimator =
            (Animator)EditorGUILayout.ObjectField(
                "Animator",
                animator,
                typeof(Animator),
                true
            );

        if (EditorGUI.EndChangeCheck())
        {
            animator = newAnimator;

            SaveObject(
                PrefAnimator,
                animator
            );
        }

        // =====================================================
        // Animation Clip
        // =====================================================

        EditorGUI.BeginChangeCheck();

        AnimationClip newClip =
            (AnimationClip)EditorGUILayout.ObjectField(
                "Animation Clip",
                clip,
                typeof(AnimationClip),
                false
            );

        if (EditorGUI.EndChangeCheck())
        {
            clip = newClip;

            SaveObject(
                PrefClip,
                clip
            );

            // Clip変更時はFrame Countを
            // Clip Framesへ自動設定
            if (clip != null)
            {
                frameCount =
                    GetClipFrameCount(
                        clip
                    );
            }
            else
            {
                frameCount = 1;
            }
        }

        // =====================================================
        // Camera
        // =====================================================

        EditorGUI.BeginChangeCheck();

        Camera newCamera =
            (Camera)EditorGUILayout.ObjectField(
                "Camera",
                renderCamera,
                typeof(Camera),
                true
            );

        if (EditorGUI.EndChangeCheck())
        {
            renderCamera = newCamera;

            SaveObject(
                PrefCamera,
                renderCamera
            );
        }

        EditorGUILayout.Space();

        // =====================================================
        // 出力画像サイズ
        // =====================================================

        width =
            EditorGUILayout.IntField(
                "Width",
                width
            );

        height =
            EditorGUILayout.IntField(
                "Height",
                height
            );

        EditorGUILayout.Space();

        // =====================================================
        // Clip情報
        // =====================================================

        if (clip != null)
        {
            int clipFrames =
                GetClipFrameCount(
                    clip
                );

            double oneFrameTime =
                clip.frameRate > 0.0f
                    ? 1.0 / clip.frameRate
                    : 0.0;

            EditorGUILayout.LabelField(
                "Clip Information",
                EditorStyles.boldLabel
            );

            EditorGUILayout.LabelField(
                "Clip Length",
                $"{clip.length:F6} sec"
            );

            EditorGUILayout.LabelField(
                "Clip Frame Rate",
                $"{clip.frameRate:F2} FPS"
            );

            EditorGUILayout.LabelField(
                "Clip Frames",
                clipFrames.ToString()
            );

            EditorGUILayout.LabelField(
                "1 Frame Time",
                $"{oneFrameTime:F6} sec"
            );

            EditorGUILayout.Space();
        }

        // =====================================================
        // Frame Count
        // =====================================================

        frameCount =
            EditorGUILayout.IntField(
                "Frame Count",
                frameCount
            );

        if (frameCount < 1)
        {
            frameCount = 1;
        }

        // =====================================================
        // Clip Framesへ戻す
        // =====================================================

        if (clip != null)
        {
            if (GUILayout.Button(
                "Frame Count = Clip Frames"
            ))
            {
                frameCount =
                    GetClipFrameCount(
                        clip
                    );
            }
        }

        EditorGUILayout.Space();

        // =====================================================
        // Export
        // =====================================================

        if (GUILayout.Button(
            "Export PNG",
            GUILayout.Height(35)
        ))
        {
            Export();
        }
    }

    // =========================================================
    // 前回設定を読み込む
    // =========================================================

    private void LoadPreviousSettings()
    {
        animator =
            LoadObject<Animator>(
                PrefAnimator
            );

        clip =
            LoadObject<AnimationClip>(
                PrefClip
            );

        renderCamera =
            LoadObject<Camera>(
                PrefCamera
            );

        // 起動時のFrame Countは
        // 保存されているClipの総フレーム数
        if (clip != null)
        {
            frameCount =
                GetClipFrameCount(
                    clip
                );
        }
        else
        {
            frameCount = 1;
        }
    }

    // =========================================================
    // Objectを保存
    // =========================================================

    private void SaveObject(
        string key,
        UnityEngine.Object obj
    )
    {
        if (obj == null)
        {
            EditorPrefs.DeleteKey(
                key
            );

            return;
        }

        GlobalObjectId globalId =
            GlobalObjectId.GetGlobalObjectIdSlow(
                obj
            );

        EditorPrefs.SetString(
            key,
            globalId.ToString()
        );
    }

    // =========================================================
    // Objectを読み込む
    // =========================================================

    private T LoadObject<T>(
        string key
    )
        where T : UnityEngine.Object
    {
        if (!EditorPrefs.HasKey(
            key
        ))
        {
            return null;
        }

        string idString =
            EditorPrefs.GetString(
                key
            );

        if (string.IsNullOrEmpty(
            idString
        ))
        {
            return null;
        }

        if (!GlobalObjectId.TryParse(
            idString,
            out GlobalObjectId globalId
        ))
        {
            return null;
        }

        UnityEngine.Object obj =
            GlobalObjectId
                .GlobalObjectIdentifierToObjectSlow(
                    globalId
                );

        return obj as T;
    }

    // =========================================================
    // Clip総フレーム数
    // =========================================================

    private int GetClipFrameCount(
        AnimationClip targetClip
    )
    {
        if (targetClip == null)
        {
            return 1;
        }

        if (targetClip.frameRate <= 0.0f)
        {
            return 1;
        }

        int frames =
            Mathf.RoundToInt(
                targetClip.length *
                targetClip.frameRate
            );

        return Mathf.Max(
            1,
            frames
        );
    }

    // =========================================================
    // Export
    // =========================================================

    private void Export()
    {
        // =====================================================
        // 入力チェック
        // =====================================================

        if (animator == null)
        {
            ShowError(
                "Animatorを指定してください。"
            );

            return;
        }

        if (clip == null)
        {
            ShowError(
                "Animation Clipを指定してください。"
            );

            return;
        }

        if (renderCamera == null)
        {
            ShowError(
                "Cameraを指定してください。"
            );

            return;
        }

        if (width <= 0)
        {
            ShowError(
                "Widthは1以上にしてください。"
            );

            return;
        }

        if (height <= 0)
        {
            ShowError(
                "Heightは1以上にしてください。"
            );

            return;
        }

        if (frameCount <= 0)
        {
            ShowError(
                "Frame Countは1以上にしてください。"
            );

            return;
        }

        if (clip.frameRate <= 0.0f)
        {
            ShowError(
                "Animation ClipのFrame Rateが不正です。"
            );

            return;
        }

        // =====================================================
        // Clip総フレーム数
        // =====================================================

        int clipTotalFrames =
            GetClipFrameCount(
                clip
            );

        // =====================================================
        // Clip Framesを超える場合
        // =====================================================

        if (frameCount > clipTotalFrames)
        {
            bool continueExport =
                EditorUtility.DisplayDialog(
                    "Frame Count",
                    $"Animation Clipのフレーム数は" +
                    $"{clipTotalFrames}です。\n\n" +
                    $"Frame Countには{frameCount}が" +
                    $"指定されています。\n\n" +
                    "Clipの長さを超えますが続行しますか？",
                    "続行",
                    "キャンセル"
                );

            if (!continueExport)
            {
                return;
            }
        }

        // =====================================================
        // 出力フォルダ
        // =====================================================

        string outputFolder =
            EditorUtility.SaveFolderPanel(
                "出力フォルダを選択",
                "",
                "AnimationFrames"
            );

        if (string.IsNullOrEmpty(
            outputFolder
        ))
        {
            return;
        }

        // =====================================================
        // 古いPNG削除
        // =====================================================

        try
        {
            string[] oldFiles =
                Directory.GetFiles(
                    outputFolder,
                    "frame_*.png"
                );

            foreach (string file in oldFiles)
            {
                File.Delete(file);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                "[PNG Exporter] " +
                "古いPNGを削除できませんでした。\n" +
                e.Message
            );
        }

        // =====================================================
        // Animator元設定
        // =====================================================

        bool oldAnimatorEnabled =
            animator.enabled;

        AnimatorCullingMode oldCullingMode =
            animator.cullingMode;

        bool oldApplyRootMotion =
            animator.applyRootMotion;

        // =====================================================
        // Camera元設定
        // =====================================================

        RenderTexture oldTarget =
            renderCamera.targetTexture;

        Color oldBackground =
            renderCamera.backgroundColor;

        CameraClearFlags oldClearFlags =
            renderCamera.clearFlags;

        RenderTexture oldActive =
            RenderTexture.active;

        // =====================================================
        // 作業用
        // =====================================================

        RenderTexture renderTexture = null;
        Texture2D texture = null;

        PlayableGraph graph = default;

        try
        {
            // =================================================
            // Animator準備
            // =================================================

            animator.enabled = true;

            animator.cullingMode =
                AnimatorCullingMode.AlwaysAnimate;

            animator.applyRootMotion =
                false;

            animator.Rebind();
            animator.Update(0.0f);

            // =================================================
            // PlayableGraph
            // =================================================

            graph =
                PlayableGraph.Create(
                    "AnimationPngExporter"
                );

            graph.SetTimeUpdateMode(
                DirectorUpdateMode.Manual
            );

            // =================================================
            // AnimationClipPlayable
            // =================================================

            AnimationClipPlayable clipPlayable =
                AnimationClipPlayable.Create(
                    graph,
                    clip
                );

            clipPlayable.SetApplyFootIK(
                false
            );

            clipPlayable.SetApplyPlayableIK(
                false
            );

            // =================================================
            // Animatorへ接続
            // =================================================

            AnimationPlayableOutput output =
                AnimationPlayableOutput.Create(
                    graph,
                    "AnimationOutput",
                    animator
                );

            output.SetSourcePlayable(
                clipPlayable
            );

            graph.Play();

            // =================================================
            // RenderTexture
            // =================================================

            renderTexture =
                new RenderTexture(
                    width,
                    height,
                    24,
                    RenderTextureFormat.ARGB32
                );

            renderTexture.antiAliasing = 1;

            renderTexture.Create();

            // =================================================
            // Texture2D
            // =================================================

            texture =
                new Texture2D(
                    width,
                    height,
                    TextureFormat.RGBA32,
                    false
                );

            // =================================================
            // Camera
            // =================================================

            renderCamera.targetTexture =
                renderTexture;

            renderCamera.clearFlags =
                CameraClearFlags.SolidColor;

            renderCamera.backgroundColor =
                new Color(
                    0.0f,
                    0.0f,
                    0.0f,
                    0.0f
                );

            // =================================================
            // 1フレーム時間
            // =================================================

            double oneFrameTime =
                1.0 /
                clip.frameRate;

            // =================================================
            // Debug
            // =================================================

            Debug.Log(
                "========================================"
            );

            Debug.Log(
                "[PNG Exporter] START"
            );

            Debug.Log(
                $"Clip          : {clip.name}"
            );

            Debug.Log(
                $"Length        : {clip.length:F6} sec"
            );

            Debug.Log(
                $"Frame Rate    : {clip.frameRate:F2} FPS"
            );

            Debug.Log(
                $"Clip Frames   : {clipTotalFrames}"
            );

            Debug.Log(
                $"Export Frames : {frameCount}"
            );

            Debug.Log(
                $"1 Frame Time  : {oneFrameTime:F6} sec"
            );

            Debug.Log(
                "========================================"
            );

            // =================================================
            // 1フレームずつ出力
            // =================================================

            for (
                int frameIndex = 0;
                frameIndex < frameCount;
                frameIndex++
            )
            {
                // =============================================
                // AnimationClip上の正確なフレーム時間
                // =============================================

                double animationTime =
                    frameIndex /
                    (double)clip.frameRate;

                animationTime =
                    Math.Min(
                        animationTime,
                        clip.length
                    );

                // =============================================
                // 指定フレームへ移動
                // =============================================

                clipPlayable.SetTime(
                    animationTime
                );

                // =============================================
                // Animation評価
                // =============================================

                graph.Evaluate(
                    0.0f
                );

                Physics.SyncTransforms();

                // =============================================
                // SkinnedMeshRenderer更新
                // =============================================

                SkinnedMeshRenderer[] renderers =
                    animator.GetComponentsInChildren
                    <SkinnedMeshRenderer>(
                        true
                    );

                foreach (
                    SkinnedMeshRenderer renderer
                    in renderers
                )
                {
                    renderer
                        .forceMatrixRecalculationPerRender =
                        true;
                }

                // =============================================
                // Debug
                // =============================================

                Debug.Log(
                    $"[PNG Exporter] " +
                    $"frame_{frameIndex + 1:00}.png | " +
                    $"AnimationFrame={frameIndex} | " +
                    $"Time={animationTime:F6}s"
                );

                // =============================================
                // Render
                // =============================================

                renderCamera.Render();

                RenderTexture.active =
                    renderTexture;

                // =============================================
                // Pixel取得
                // =============================================

                texture.ReadPixels(
                    new Rect(
                        0,
                        0,
                        width,
                        height
                    ),
                    0,
                    0
                );

                texture.Apply();

                // =============================================
                // PNG
                // =============================================

                byte[] png =
                    texture.EncodeToPNG();

                string fileName =
                    $"frame_{frameIndex + 1:00}.png";

                string path =
                    Path.Combine(
                        outputFolder,
                        fileName
                    );

                File.WriteAllBytes(
                    path,
                    png
                );
            }

            // =================================================
            // 完了
            // =================================================

            Debug.Log(
                "========================================"
            );

            Debug.Log(
                "[PNG Exporter] COMPLETE"
            );

            Debug.Log(
                $"Clip          : {clip.name}"
            );

            Debug.Log(
                $"Clip Frames   : {clipTotalFrames}"
            );

            Debug.Log(
                $"Export Frames : {frameCount}"
            );

            Debug.Log(
                $"Size          : {width} x {height}"
            );

            Debug.Log(
                "========================================"
            );

            EditorUtility.DisplayDialog(
                "Export Complete",
                $"Animation : {clip.name}\n" +
                $"Frame Rate : {clip.frameRate:F2} FPS\n" +
                $"Clip Frames : {clipTotalFrames}\n" +
                $"Export Frames : {frameCount}\n" +
                $"Size : {width} x {height}",
                "OK"
            );
        }
        catch (Exception e)
        {
            Debug.LogException(
                e
            );

            EditorUtility.DisplayDialog(
                "Export Error",
                e.Message,
                "OK"
            );
        }
        finally
        {
            // =================================================
            // Graph破棄
            // =================================================

            if (graph.IsValid())
            {
                graph.Destroy();
            }

            // =================================================
            // Camera復元
            // =================================================

            renderCamera.targetTexture =
                oldTarget;

            renderCamera.backgroundColor =
                oldBackground;

            renderCamera.clearFlags =
                oldClearFlags;

            RenderTexture.active =
                oldActive;

            // =================================================
            // Animator復元
            // =================================================

            animator.Rebind();

            animator.cullingMode =
                oldCullingMode;

            animator.applyRootMotion =
                oldApplyRootMotion;

            animator.enabled =
                oldAnimatorEnabled;

            if (animator.enabled)
            {
                animator.Update(
                    0.0f
                );
            }

            // =================================================
            // Texture破棄
            // =================================================

            if (texture != null)
            {
                DestroyImmediate(
                    texture
                );
            }

            if (renderTexture != null)
            {
                renderTexture.Release();

                DestroyImmediate(
                    renderTexture
                );
            }

            AssetDatabase.Refresh();

            SceneView.RepaintAll();
        }
    }

    // =========================================================
    // Error
    // =========================================================

    private void ShowError(
        string message
    )
    {
        Debug.LogError(
            "[PNG Exporter] " +
            message
        );

        EditorUtility.DisplayDialog(
            "Error",
            message,
            "OK"
        );
    }
}