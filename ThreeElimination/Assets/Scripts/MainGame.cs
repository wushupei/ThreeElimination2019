
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MainGame : MonoBehaviour
{
    public static MainGame Instance; //单例

    private CellController[,] cells; //二维数组保存所有格子
    private List<CellController> CellList = new List<CellController>(); //需要被删除图片的格子
    [SerializeField] private Animator scoreAnim;
    [SerializeField] private Animator lifeAnim;
    [SerializeField] private Transform slot;
    [SerializeField] private Transform effects;
    [HideInInspector] public int score; //分数
    [HideInInspector] public int life; //生命值
    [HideInInspector] public float cellsdis; //上下格子的距离
    [HideInInspector] public Image[] imageAssets; //所有图片资源
    [HideInInspector] public bool drap = true;
    [HideInInspector] public int doubleHit; //连击数  

    private void Start()
    {
        InitGame();
    }
    private void InitGame() //初始化游戏
    {
        Instance = this;
        life = byte.MaxValue;
        lifeAnim.GetComponent<Text>().text = life.ToString();
        imageAssets = Resources.LoadAll<Image>("Greens"); //加载所有图片 
        //根据格子尺寸设置图片大小
        float width = slot.GetComponent<GridLayoutGroup>().cellSize.x;
        foreach (var item in imageAssets)
        {
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width + 30);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, width + 30);
        }
        int column = slot.GetComponent<GridLayoutGroup>().constraintCount; //获取每行格子数(列数)
        int row = slot.childCount / column; //总数除以列数的到行数
        cells = new CellController[row, column]; //初始化宽和高一样的二维数组
        //拿到格子添加进二维数组
        for (int i = 0; i < slot.childCount; i++)
        {
            cells[i / column, i % column] = slot.GetChild(i).GetComponent<CellController>();
        }
        cellsdis = Vector3.Distance(cells[0, 0].transform.position, cells[1, 0].transform.position);

        //为每个格子初始化图片(避免连续三个相同)
        for (int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                //将加载的图片放入集合
                List<Image> images = new List<Image>(imageAssets);
                //发现上面两个格子图片相同,则从集合移除该图片
                if (i >= 2 && cells[i - 1, j].imageType == cells[i - 2, j].imageType)
                    images.Remove(imageAssets[cells[i - 1, j].imageType]);

                //发现左面两个格子图片相同,则从集合移除该图片
                if (j >= 2 && cells[i, j - 1].imageType == cells[i, j - 2].imageType)
                    images.Remove(imageAssets[cells[i, j - 1].imageType]);

                //添加随机图片(如果第0位是钻石,则减小生成几率 )
                int index = Array.IndexOf(imageAssets, images[0]) == 0 ?
                    UnityEngine.Random.Range(0, 20) < 1 ? 0 : UnityEngine.Random.Range(1, images.Count) :
                    UnityEngine.Random.Range(0, images.Count);

                Instantiate(images[index], cells[i, j].transform.position, Quaternion.identity, cells[i, j].transform);
                //生成后根据图片在资源数组的索引确定格子的图片类型
                int imageType = Array.IndexOf(imageAssets, images[index]);
                cells[i, j].InitCell(cells, i, j, imageType, effects, width);
            }
        }
    }
    public void AddCell(CellController cell) //添加需要删除图片的格子,并检查是否有道具
    {
        if (!CellList.Contains(cell))
        {
            CellList.Add(cell);
            if (cell.imageType == 0)
                cell.CheckPropCell(cell);
        }
    }
    public void ClearCell() //删除图片
    {
        foreach (var item in CellList)
        {
            item.PlayDeleteAnima(); //播放格子删除动画
        }
    }
    private void AddScore() //增加分数
    {
        doubleHit += CellList.Count - (doubleHit == 0 ? 2 : 0);
        score += doubleHit * 10;
        scoreAnim.Play("Score");
    }
    public void subLife() //减少生命值
    {
        life--;
        lifeAnim.Play("Life");
    }
    public void CheakCelloming() //检测所有格子是否图片都归位
    {
        foreach (var item in cells)
        {
            if (item.homing == false)
                return;
        }
        drap = true; //都归位后可以拖动
        //检测横竖相同的格子
        foreach (var item in cells)
        {
            item.CheckIdentical(true);
            item.CheckIdentical(false);
        }
        ClearCell(); //清除相同图片        
    }
    public void CheakCellDeleted() //检测所有格子是否图片都删除
    {
        foreach (var item in CellList)
        {
            if (item.deleted == false)
                return;
        }
        //所有图片都删除完毕加分,清除集合,刷新图片
        if (CellList.Count > 0)
        {
            AddScore();
            CellList.Clear();
            StartCoroutine(RefreshImage());
        }
    }
    private IEnumerator RefreshImage() //刷新图片
    {
        yield return null;
        //从下往上,从左往右进行遍历,找出空格子
        for (int j = 0; j < cells.GetLength(1); j++)
        {
            for (int i = cells.GetLength(0) - 1; i >= 0; i--)
            {
                if (cells[i, j].transform.childCount == 0)
                {
                    cells[i, j].GetImageFromAbove(); //发现空格子从上面获取
                }
            }
        }
    }
}
