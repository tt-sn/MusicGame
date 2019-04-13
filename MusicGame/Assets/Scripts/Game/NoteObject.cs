using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common.Data;

namespace Game
{
    public class NoteObject : MonoBehaviour
    {
        public float baseY;

        [SerializeField]
        Image image;

        SceneController sceneController;
        AudioSource bgm;
        SongData.Note note;
        float positionX;

        public int NoteNumber
        {
            //３項演算子
            //ノーツがアクティブならノーツ番号を返し，非アクティブなら最小値を返す
            get { return gameObject.activeSelf ? note.NoteNumber : int.MinValue; }
        }

        //ノーツとBGMのフレーム差の絶対値を返す
        public float AbsoluteTimeDiff
        {
            get { return Mathf.Abs(note.Time - bgm.time); }
        }

        //毎フレーム呼び出される
        void Update()
        {
            var timeDiff = note.Time - bgm.time;
            //ノーツがバッド判定より外側（要は判定オーバー）なら
            //OneNoteMissを呼び出し，ノーツを非表示にする
            if (timeDiff < -SceneController.BAD_BORDER)
            {
                sceneController.OnNoteMiss(NoteNumber);
                gameObject.SetActive(false);
            }
            //ノーツを降らせる
            GetComponent<RectTransform>().localPosition = new Vector3(positionX,
                                                  baseY + timeDiff * 800f,
                                                  transform.localPosition.z);
        }


        /// <summary>
        /// ノーツを生成するクラス
        /// </summary>
        /// <param name="sceneController"></param>
        /// <param name="bgm"></param>
        /// <param name="note"></param>
        /// <param name="positionX"></param>
        public void Initialize(SceneController sceneController, AudioSource bgm, SongData.Note note, float positionX)
        {
            gameObject.SetActive(true);

            this.sceneController = sceneController;
            this.bgm = bgm;
            this.note = note;
            this.positionX = positionX;
            //レーンによってノーツの色を変える
            switch (note.NoteNumber)
            {
                case 1:
                case 3:
                case 5:
                    image.color = Color.green;
                    break;
                default:
                    image.color = Color.white;
                    break;
            }

            Update();
        }
    }
}