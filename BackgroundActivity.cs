﻿using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Android.Content;

namespace TabletArtco
{
    [Activity(Label = "BackgroundActivity")]
    public class BackgroundActivity : Activity, DataSource, Delegate
    {

        private int mItemW;
        private int mItemH;
        private int mIndex;

        private Block block;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Window.SetFlags(Android.Views.WindowManagerFlags.Fullscreen, Android.Views.WindowManagerFlags.Fullscreen);
            SetContentView(Resource.Layout.activity_grid);
            RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
            InitData();
            InitView();
        }

        private void InitData() {
            Intent intent = this.Intent;
            Bundle bundle = intent.GetBundleExtra("bundle");
            if (bundle != null)
            {
                int index = bundle.GetInt("index");
                int row = bundle.GetInt("row");
                int column = bundle.GetInt("column");
                if (index != -1 && index < Project.mSprites.Count)
                {
                    ActivatedSprite activatedSprite = Project.mSprites[index];
                    if (row != -1 && row < activatedSprite.mBlocks.Count)
                    {
                        List<Block> list = activatedSprite.mBlocks[row];
                        if (column != -1 && column < list.Count)
                        {
                            block = list[column];
                        }
                    }
                }
            }
            
        }

        private void InitView()
        {
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
                Resource.Drawable.search_bg, Resource.Drawable.bs_themeBackground_tab, Resource.Drawable.bs_cartoonbackground_tab,
                Resource.Drawable.bs_reallifebackground_tab, Resource.Drawable.bs_momochungbackground_tab, Resource.Drawable.bs_userbackground_tab
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
                    LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(itemW, itemH);
                    lp.LeftMargin = margin / 3;
                    ImageView imgIv = new ImageView(this);
                    imgIv.Tag = i;
                    imgIv.LayoutParameters = lp;
                    imgIv.SetImageResource(resIds[i]);
                    topView.AddView(imgIv);
                    imgIv.Click += (t, e) =>
                    {
                        int tag = (int)(((ImageView)t).Tag);
                        mIndex = tag+4;
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
            mItemH = (int)(mItemW * 170.0 / 250);

            GridView gridView = FindViewById<GridView>(Resource.Id.gridview);
            gridView.SetColumnWidth(200);
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
            List<List<Background>> backgrounds = Background._backgrounds;
            if (mIndex < backgrounds.Count)
            {
                return backgrounds[mIndex].Count;
            }
            return 0;
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
            List<List<Background>> backgrounds = Background._backgrounds;
            if (mIndex >= backgrounds.Count)
            {
                return;
            }
            List<Background> list = backgrounds[mIndex];
            Background background = list[position];
            ViewHolder viewHolder = (ViewHolder)contentView.Tag;
            Glide.With(this).Load(GlideUtil.GetGlideUrl(background.remotePreviewImgPath)).Into(viewHolder.imgIv);
            viewHolder.txtTv.Text = background.name;
            viewHolder.txtTv.Tag = position;
        }

        public void ClickItem(int position)
        {
            List<List<Background>> backgrounds = Background._backgrounds;
            if (mIndex >= backgrounds.Count)
            {
                return;
            }
            List<Background> list = backgrounds[mIndex];
            Background background = list[position];
            Intent intent = new Intent();
            Bundle bundle = new Bundle();
            bundle.PutString("model", background.ToString());
            intent.PutExtra("bundle", bundle);
            if (block != null)
            {
                block.text = background.name;
                block.varName = background.remotePreviewImgPath;
                block.varValue = background.remoteVideoPath;
            }
            SetResult(Result.Ok, intent);
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
