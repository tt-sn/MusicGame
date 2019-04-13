using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Common;
using Common.Data;

namespace Game
{
    public class SceneController : MonoBehaviour
    {
        public const float PRE_NOTE_SPAWN_TIME = 3f;
        public const float PERFECT_BORDER = 0.05f;
        public const float GREAT_BORDER = 0.1f;
        public const float GOOD_BORDER = 0.2f;
        public const float BAD_BORDER = 0.5f;

        [SerializeField]
        AudioManager audioManager;      //ボタンを押した音
        [SerializeField]
        Button[] noteButtons;           //キーの格納
        [SerializeField]
        Color defaultButtonColor;       //レーンの色
        [SerializeField]
        Color highlightButtonColor;     //ボタンが押された時のレーンの色
        [SerializeField]
        TextAsset songDataAsset;
        [SerializeField]
        Transform noteObjectContainer;  //ノーツの座標
        [SerializeField]
        NoteObject noteObjectPrefab;
        [SerializeField]
        Transform messageObjectContainer;
        [SerializeField]
        MessageObject messageObjectPrefab;
        [SerializeField]
        Transform baseLine;
        [SerializeField]
        GameObject gameOverPanel;       //ゲームオーバーになったときの画面
        [SerializeField]
        Button retryButton;             //リトライボタン
        [SerializeField]
        Text scoreText;
        [SerializeField]
        Text lifeText;

        float previousTime = 0f;
        SongData song;
        Dictionary<Button, int> lastTappedMilliseconds = new Dictionary<Button, int>();
        //予め作成するノーツの格納するリストの作成
        List<NoteObject> noteObjectPool = new List<NoteObject>();
        List<MessageObject> messageObjectPool = new List<MessageObject>();
        int life;
        int score;

        //押すボタンの配列
        KeyCode[] keys = new KeyCode[]
        {
            KeyCode.Z, KeyCode.S, KeyCode.X, KeyCode.D, KeyCode.C, KeyCode.F, KeyCode.V
        };

        //体力設定
        int Life
        {
            //体力が減るとき
            set
            {
                life = value;
                if (life <= 0)
                {
                    life = 0;
                    gameOverPanel.SetActive(true);
                }
                lifeText.text = string.Format("Life: {0}", life);
            }
            get { return life; }
        }

        int Score
        {
            set
            {
                score = value;
                scoreText.text = string.Format("Score: {0}", score);
            }
            get { return score; }
        }

        void Start()
        {
            // フレームレート設定
            Application.targetFrameRate = 60;

            //初期値の設定
            Score = 0;
            Life = 10;
            retryButton.onClick.AddListener(OnRetryButtonClick);

            // ボタンのリスナー設定と最終タップ時間の初期化
            for (var i = 0; i < noteButtons.Length; i++)
            {
                noteButtons[i].onClick.AddListener(GetOnNoteButtonClickAction(i));
                lastTappedMilliseconds.Add(noteButtons[i], 0);
            }

            //オブジェクトを作成，リストに格納
            // ノートオブジェクトのプール
            for (var i = 0; i < 100; i++)
            {
                var obj = Instantiate(noteObjectPrefab, noteObjectContainer);
                //座標を（ノーツの順番）を決定
                obj.baseY = baseLine.localPosition.y;
                obj.gameObject.SetActive(false);
                //生成したオブジェクトを初期位置（リスト）に追加
                noteObjectPool.Add(obj);
            }
            //ノーツを非表示(not active)にする
            noteObjectPrefab.gameObject.SetActive(false);

            // メッセージオブジェクトのプール
            for (var i = 0; i < 50; i++)
            {
                var obj = Instantiate(messageObjectPrefab, messageObjectContainer);
                obj.baseY = baseLine.localPosition.y;
                obj.gameObject.SetActive(false);
                messageObjectPool.Add(obj);
            }
            messageObjectPrefab.gameObject.SetActive(false);

            // 楽曲データのロード
            song = SongData.LoadFromJson(songDataAsset.text);

            audioManager.bgm.PlayDelayed(1f);
        }

        void Update()
        {
            // キーボード入力も可能に
            for (var i = 0; i < keys.Length; i++)
            {
                //押されたキーを確認
                if (Input.GetKeyDown(keys[i]))
                {
                    noteButtons[i].onClick.Invoke();
                }
            }

            // ノートを生成
            var bgmTime = audioManager.bgm.time;
            foreach (var note in song.GetNotesBetweenTime(previousTime + PRE_NOTE_SPAWN_TIME, bgmTime + PRE_NOTE_SPAWN_TIME))
            {
                //ノーツのプールから非アクティブな最初のノーツを取り出す
                var obj = noteObjectPool.FirstOrDefault(x => !x.gameObject.activeSelf);
                //予め指定してあるノーツの座標を取得する
                var positionX = noteButtons[note.NoteNumber].transform.localPosition.x;
                //取り出したノーツを実際のオブジェクトとして生成
                obj.Initialize(this, audioManager.bgm, note, positionX);
            }
            previousTime = bgmTime;
        }

        //判定----------------------------------------------------
        void OnNotePerfect(int noteNumber)
        {
            ShowMessage("Perfect", Color.yellow, noteNumber);
            Score += 1000;
        }

        void OnNoteGreat(int noteNumber)
        {
            ShowMessage("Great", Color.magenta, noteNumber);
            Score += 500;
        }

        void OnNoteGood(int noteNumber)
        {
            ShowMessage("Perfect", Color.green, noteNumber);
            Score += 300;
        }

        void OnNoteBad(int noteNumber)
        {
            ShowMessage("Bad", Color.gray, noteNumber);
            Life--;
        }

        public void OnNoteMiss(int noteNumber)
        {
            ShowMessage("Miss", Color.black, noteNumber);
            Life--;
        }
        //---------------------------------------------------------

        void ShowMessage(string message, Color color, int noteNumber)
        {
            if (gameOverPanel.activeSelf)
            {
                return;
            }
            //ノーツ（というかレーン？）のx座標を取得
            var positionX = noteButtons[noteNumber].transform.localPosition.x;
            //メッセージ用のプールから非アクティブのものを探す
            var obj = messageObjectPool.FirstOrDefault(x => !x.gameObject.activeSelf);
            obj.Initialize(message, color, positionX);
        }

        /// <summary>
        /// ボタンのフォーカスを外します
        /// </summary>
        /// <returns>The coroutine.</returns>
        /// <param name="button">Button.</param>
        IEnumerator DeselectCoroutine(Button button)
        {
            yield return new WaitForSeconds(0.1f);
            if (lastTappedMilliseconds[button] <= DateTime.Now.Millisecond - 100)
            {
                button.image.color = defaultButtonColor;
            }
        }

        /// <summary>
        /// ノート（音符）に対応したボタン押下時のアクションを返します
        /// </summary>
        /// <returns>The on note button click action.</returns>
        /// <param name="noteNo">Note no.</param>
        UnityAction GetOnNoteButtonClickAction(int noteNo)
        {
            //ラムダ式を勉強してね
            return () =>
            {
                //rゲームオーバーパネルが表示されていたら終了する
                if (gameOverPanel.activeSelf)
                {
                    return;
                }

                //ボタンを押したときの音を再生する（タップ音）
                audioManager.notes[noteNo].Play();
                //レーンカラーを変更する
                noteButtons[noteNo].image.color = highlightButtonColor;
                //( https://doruby.jp/users/ino/entries/%E3%80%90C--Unity%E3%80%91%E3%82%B3%E3%83%AB%E3%83%BC%E3%83%81%E3%83%B3(Coroutine)%E3%81%A8%E3%81%AF%E4%BD%95%E3%81%AA%E3%81%AE%E3%81%8B )ここ読んで
                //コルーチンを開始する（コルーチンの処理が終わり次第要素を非Activeにする(要素)）
                StartCoroutine(DeselectCoroutine(noteButtons[noteNo]));
                //ノーツにの番号に現在の時間を格納する
                lastTappedMilliseconds[noteButtons[noteNo]] = DateTime.Now.Millisecond;

                //.Whare（指定した条件の要素のみを取り出す
                //.OrderBy（リストの要素を昇順ソートする）
                //.FirstOrDefault（一番最初にある要素を取得する：要素が空であればnullを返す）
                var targetNoteObject = noteObjectPool.Where(x => x.NoteNumber == noteNo)
                                                     .OrderBy(x => x.AbsoluteTimeDiff)
                                                     .FirstOrDefault(x => x.AbsoluteTimeDiff <= BAD_BORDER);

                //ボタンを押したときに，判定範囲内の要素がなければノーツの判定処理を行わない
                if (null == targetNoteObject)
                {
                    return;
                }

                //判定のフレーム範囲内にあった処理を実行する
                var timeDiff = targetNoteObject.AbsoluteTimeDiff;
                if (timeDiff <= PERFECT_BORDER)
                {
                    OnNotePerfect(targetNoteObject.NoteNumber);
                }
                else if (timeDiff <= GREAT_BORDER)
                {
                    OnNoteGreat(targetNoteObject.NoteNumber);
                }
                else if (timeDiff <= GOOD_BORDER)
                {
                    OnNoteGood(targetNoteObject.NoteNumber);
                }
                else
                {
                    OnNoteBad(targetNoteObject.NoteNumber);
                }
                targetNoteObject.gameObject.SetActive(false);
            };
        }

        void OnRetryButtonClick()
        {
            SceneManager.LoadScene("Game");
        }
    }
}
