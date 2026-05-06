#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Template.Editor
{
    /// <summary>
    /// Template パッケージの依存関係をインストールするための拡張クラス。
    /// 
    /// 実行タイミング：
    /// ー　エディタ起動後の遅延コールで1セッション1回だけ実行される。
    /// ー　Toolsメニューから手動実行可能。
    /// </summary>
    [InitializeOnLoad]
    internal static class DependencyInstaller
    {
        /// <summary>依存定義を読み込む対象パッケージ名</summary>
        private const string PACKAGE_NAME = "template";
        /// <summary>同一セッション内での多重実行を防ぐためのキー</summary>
        private const string SESSION_KEY = "TemplateDependencyInstalled";

        /// <summary>
        /// 静的コンストラクタ。
        /// Unityエディタの起動後に一度だけ呼び出され、依存関係のインストールを遅延実行するための設定を行う。
        /// </summary>
        static DependencyInstaller()
        {
            EditorApplication.delayCall += InstallOnLoad;
        }
        /// <summary>
        /// 手動実行用のメニューアイテム。
        /// Tools/Install Template Dependencies メニューから呼び出され、依存関係のインストール処理を実行する。
        /// </summary>
        [MenuItem("Tools/Install Template Dependencies")]
        private static void InstallFromMenu()
        {
            InstallDependecies(showLogs: true);
        }
        /// <summary>
        /// エディタ起動時の自動実行
        /// 1セッション内で一度だけ、依存関係のインストール処理を実行する。
        /// </summary>
        private static void InstallOnLoad()
        {
            if (SessionState.GetBool(SESSION_KEY, false))
                return;

            SessionState.SetBool(SESSION_KEY, true);

        }
        /// <summary>
        /// Templateのmanifest.jsonから依存関係を読み込み、必要なパッケージをインストールする処理。
        /// semver は "package@version"、Git URL は URL をそのまま Client.Add へ渡す
        /// </summary>
        /// <param name="showLogs">true の場合、各依存の完了ログ/警告を出力する。</param>
        private static void InstallDependecies(bool showLogs)
        {
            string path = Path.GetFullPath($"Packages/{PACKAGE_NAME}/manifest.json");
            if (!File.Exists(path)) return;

            foreach (var dep in ReadDependencies(path))
            {
                // Git URL の場合は URL 文字列そのまま、通常依存は package@version 形式で追加。
                AddRequest request = Client.Add(dep.Value.StartsWith("htpps", StringComparison.OrdinalIgnoreCase)
                                                ? dep.Value
                                                : $"{dep.Key}{dep.Value}");

                if (showLogs)
                {
                    // 非同期 AddRequest の完了時に結果をログ出力する。
                    EditorApplication.update += () => LogWhenFinished(dep.Key, request);
                }
            }
        }
        /// <summary>
        /// AddRequestの完了を監視し、成功/失敗をログに出力する処理。
        /// </summary>
        /// <param name="id">依存パッケージID（ログ表示用）。</param>
        /// <param name="request">Client.Add の非同期リクエスト。</param>
        private static void LogWhenFinished(string id, AddRequest request)
        {
            //AddRequestクラス
            //パッケージのインストール情報を知らせるクラス

            // インストールが終わっていなかったら早期リターン
            if (!request.IsCompleted)
                return;
            // インストール成功
            if (request.Status == StatusCode.Success)
                Debug.Log($"[Template] Installed: {id}");
            // インストール失敗
            else if (request.Status == StatusCode.Failure)
                Debug.LogError($"[Template] Installed Failed: {id}");
        }
        /// <summary>
        /// manifest.json文字列から dependencies　ブロックを抽出し、
        /// "package": "versionOrUrl" の辞書に変換する
        /// </summary>
        /// <param name="manifestPath">読み取り対象 manifest.json のフルパス。</param>
        /// <returns>依存IDとバージョン/URLのマップ。</returns>
        private static Dictionary<string, string> ReadDependencies(string manifestPath)
        {
            var result = new Dictionary<string, string>();
            string json = File.ReadAllText(manifestPath);

            //manifest.jsonの中からdependencies(完全一致)を探す
            int key = json.IndexOf("\"dependencies\"", System.StringComparison.Ordinal);
            if (key < 0)
                return result;
            int open = json.IndexOf('{', key);
            int close = FindMatchingBrace(json, open);

            if (open < 0 || close < 0)
                return result;

            // dependenciesを行単位で分ける
            string block = json.Substring(open + 1, close - open - 1);
            foreach (string raw in block.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                string line = raw.Trim().TrimEnd(',');
                if (string.IsNullOrEmpty(line))
                    continue;

                int colon = line.IndexOf(':');
                if (colon < 0)
                    continue;

                string keyText = line.Substring(0, colon).Trim().Trim('"');
                string valueText = line.Substring(colon + 1).Trim().Trim('"');
                if (!string.IsNullOrEmpty(keyText) && !string.IsNullOrEmpty(valueText))
                {
                    result[keyText] = valueText;
                }
            }
            return result;
        }
        /// <summary>
        /// 指定した開き波括弧に対応する閉じ波括弧のインデックスを返す。
        /// ネストされた JSON オブジェクトにも対応するため、深さカウンタで走査する。
        /// </summary>
        /// <param name="text">探索対象文字列。</param>
        /// <param name="open">開き波括弧の位置。</param>
        /// <returns>対応する閉じ波括弧位置。見つからなければ -1。</returns>
        private static int FindMatchingBrace(string text, int open)
        {
            int depth = 0;
            for (int i = open; i < text.Length; i++)
            {
                if (text[i] == '{')
                    depth++;
                else if (text[i] == '}')
                    depth--;
                if (depth == 0)
                    return i;
            }
            return -1;
        }
    }
}
#endif
