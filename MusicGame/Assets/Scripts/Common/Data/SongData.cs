using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Common.Data
{

    public class SongData
    {
        //ノーツタイプ型の宣言（判定）
        public enum NoteType
        {
            Whole = 1,
            Half = 2,
            Quarter = 4,
            Eighth = 8,
            Sixteenth = 16,
            ThirtySecond = 32,
        }

        //BPM設定
        public int bpm = 120;

        public NoteType minNoteType = NoteType.Sixteenth;

        //生成したノーツを格納するリストを作成    
        [SerializeField]
        List<Note> notes = new List<Note>();

        //ノーツの残数を確認する
        public bool HasNote
        {
            get { return notes.Count > 0; }
        }

        //jsonから楽曲データを取得
        public static SongData LoadFromJson(string json)
        {
            //FromJson:jsonからオブジェクトを作成
            return JsonUtility.FromJson<SongData>(json);
        }

        public void AddNote(float time, int noteNumber)
        {
            //判定を決定:minNoteTypeにはフレーム幅が入ってるので，その半分（つまり前後）を決定.
            var minNoteLength = 2f / (float)minNoteType;
            //フレーム数を整数に直す（Mathf.Round():四捨五入）
            var roundedTime = Mathf.Round(time / minNoteLength) * minNoteLength;
            //重複ノールを確認，なければリストにノーツを格納
            //.Any()指定した要素を満たすものがあるか判断する
            if (!notes.Any(x => Mathf.Abs(x.Time - roundedTime) <= Mathf.Epsilon && x.NoteNumber == noteNumber))
            {
                notes.Add(new Note(roundedTime, noteNumber));
            }
        }
        /// <summary>
        /// 指定時間内のうちのnotesを取り出す //LINQ
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IEnumerable<Note> GetNotesBetweenTime(float start, float end)
        {
            //.Where() 指定した要素を取り出す
            return notes.Where(x => start < x.Time && x.Time <= end);
        }

        public void ClearNotes()
        {
            notes.Clear();
        }

        //ノーツが持つ構造体
        [Serializable]
        public struct Note
        {
            [SerializeField]
            float time;
            [SerializeField]
            int noteNumber;

            //判定の時間（降ってくる時間）
            public float Time
            {
                get { return time; }
            }
            //ノーツの番号
            public int NoteNumber
            {
                get { return noteNumber; }
            }

            //プロパティのset部分，Noteの中身の変数に値を入れる
            public Note(float time, int noteNumber)
            {
                this.time = time;
                this.noteNumber = noteNumber;
            }
        }
    }


}
