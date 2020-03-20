﻿using System.Collections.Generic;
using System.IO;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;

namespace TabletArtco
{
    [Activity(Label = "ProjectActivity")]
    public class ProjectActivity : Activity, DataSource, Delegate
    {
        private int mItemW;
        private int mItemH;
        private int mIndex;

        private List<string[]> filePathList = new List<string[]>();
        private List<string[]> fileNameList = new List<string[]>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Window.SetFlags(Android.Views.WindowManagerFlags.Fullscreen, Android.Views.WindowManagerFlags.Fullscreen);
            SetContentView(Resource.Layout.activity_grid);
            RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
            InitView();
        }

        private void InitView()
        {
            string[] path = new string[]
            {
                UserDirectoryPath.projectPath,
                UserDirectoryPath.objectPath
            };

            for(int i= 0; i < path.Length; i++)
            {
                if (Directory.Exists(path[i]))
                {
                    string[] pathList = Directory.GetFiles(path[i]);
                    filePathList.Add(pathList);

                    string[] nameList = new string[pathList.Length];
                    for (int j = 0; j < nameList.Length; j++)
                    {
                        nameList[j] = Path.GetFileNameWithoutExtension(pathList[j]);
                    }
                    fileNameList.Add(nameList);
                }
                else
                {
                    filePathList.Add(new string[0]);
                    fileNameList.Add(new string[0]);
                }
            }

            int width = ScreenUtil.ScreenWidth(this);
            int height = ScreenUtil.ScreenHeight(this);
            int margin = (int)(20 / 1280.0 * width);
            int w = width - margin * 2;
            int topH = (int)(60 / 800.0 * height);
            int conH = height - topH - margin;

            ScrollView scrollView = FindViewById<ScrollView>(Resource.Id.scrollView);
            LinearLayout.LayoutParams svParams = (LinearLayout.LayoutParams)scrollView.LayoutParameters;
            svParams.Width = width;
            svParams.Height = topH;
            scrollView.LayoutParameters = svParams;
            scrollView.SetPadding(margin, 0, margin, 0);
            LinearLayout topView = FindViewById<LinearLayout>(Resource.Id.grid_top_view);
            int[] resIds = {
                Resource.Drawable.search_bg, Resource.Drawable.ebs_codingpractice_tab, Resource.Drawable.ebs_codingapplied1_tab
            };
            int editTvH = (int)(30 / 60.0 * topH);
            int editTvW = (int)(166 / 35.0 * editTvH);
            int itemH = (int)(34 / 60.0 * topH);
            int itemW = (int)(92 / 35.0 * itemH);
            for (int i = 0; i < resIds.Length; i++)
            {
                if (i == 0)
                {
                    LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(editTvW, editTvH);
                    EditText searchEt = new EditText(this);
                    searchEt.LayoutParameters = lp;
                    searchEt.SetBackgroundResource(resIds[i]);
                    topView.AddView(searchEt);
                }
                else
                {
                    LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(i + 2 >= resIds.Length ? (int)(125 / 92.0 * itemW) : itemW, itemH);
                    lp.LeftMargin = margin / 3;
                    ImageView imgIv = new ImageView(this);
                    imgIv.Tag = i;
                    imgIv.LayoutParameters = lp;
                    imgIv.SetImageResource(resIds[i]);
                    topView.AddView(imgIv);
                    imgIv.Click += (t, e) =>
                    {
                        int tag = (int)(((ImageView)t).Tag);
                        mIndex = tag - 1;
                        UpdateView();
                    };
                }
            }

            int spacing = 20;
            LinearLayout conView = FindViewById<LinearLayout>(Resource.Id.grid_wrapper_view);
            LinearLayout.LayoutParams conParams = (LinearLayout.LayoutParams)conView.LayoutParameters;
            conParams.Width = w;
            conParams.Height = conH;
            conParams.LeftMargin = margin;
            conView.LayoutParameters = conParams;
            conView.SetPadding(spacing, spacing, spacing, spacing);

            int columnCount = 4;
            mItemW = (int)((w - (columnCount + 1) * spacing * 1.0) / columnCount);
            mItemH = mItemW;

            GridView gridView = FindViewById<GridView>(Resource.Id.gridview);
            gridView.SetNumColumns(columnCount);
            gridView.SetVerticalSpacing(spacing);
            gridView.SetHorizontalSpacing(spacing);
            gridView.Adapter = new GridAdapter((DataSource)this, (Delegate)this);
        }

        private void UpdateView()
        {
            GridView gridView = FindViewById<GridView>(Resource.Id.gridview);
            GridAdapter adapter = (GridAdapter)gridView.Adapter;
            adapter.NotifyDataSetChanged();
        }

        // Delegate interface
        public int GetItemsCount(Java.Lang.Object adapter)
        {
            return fileNameList[mIndex].Length;
        }

        public View GetItemView(Java.Lang.Object adapter, ViewGroup parent)
        {
            View convertView = LayoutInflater.From(this).Inflate(Resource.Layout.item_background, parent, false);
            ViewUtil.SetViewSize(convertView, mItemW, mItemH);
            ViewHolder holder = new ViewHolder();
            holder.bgIv = convertView.FindViewById<ImageView>(Resource.Id.selected_material_bgIv);
            holder.imgIv = convertView.FindViewById<ImageView>(Resource.Id.selected_material_imgIv);
            holder.txtTv = convertView.FindViewById<TextView>(Resource.Id.sprite_tv);
            convertView.Tag = holder;
            convertView.Click += (t, e) =>
            {
                ViewHolder viewHolder = (ViewHolder)(((View)t).Tag);
                int position = (int)viewHolder.txtTv.Tag;
                ClickItem(position);
            };
            return convertView;
        }

        public void UpdateItemView(Java.Lang.Object adapter, View contentView, int position)
        {
            ViewHolder viewHolder = (ViewHolder)contentView.Tag;
            if(mIndex==0)
                viewHolder.imgIv.SetImageResource(Resource.Drawable.PO_ProjectBack);
            else
                viewHolder.imgIv.SetImageResource(Resource.Drawable.PO_SpriteBack);
            viewHolder.txtTv.Text = fileNameList[mIndex][position];
            viewHolder.txtTv.Tag = position;
        }

        public void ClickItem(int position)
        {
            bool result = new ArtcoObject(this).LoadObject(filePathList[mIndex][position]);
            Intent intent = new Intent();
            if (result)
                SetResult(Result.Ok, intent);
            else
                SetResult(Result.Canceled);

            Finish();
        }

        //定义ViewHolder内部类，用于对控件实例进行缓存
        class ViewHolder : Java.Lang.Object
        {
            public ImageView bgIv;
            public ImageView imgIv;
            public TextView txtTv;
        }
    }
}