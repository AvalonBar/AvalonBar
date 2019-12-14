using Sidebar.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Sidebar
{
    public static class TileManager
    {
        public static List<Tile> Tiles = new List<Tile>();
        public static Panel TileContainer { get; set; }
        public static Panel PinnedTileContainer { get; set; }

        public static TileState GetTileState(TileControl tile)
        {
            TileState tileState = new TileState();

            tileState.Name = Path.GetFileName(tile.ParentTile.Path);
            tileState.IsMinimized = tile.minimized;
            tileState.IsPinned = tile.pinned;
            tileState.Height = tile.Height;
            tileState.Order = TileContainer.Children.IndexOf(tile);
            if (tile.pinned)
            {
                tileState.Order = PinnedTileContainer.Children.IndexOf(tile);
            }
            if (tileState.IsMinimized)
            {
                tileState.Height = tile.normalHeight;
            }

            return tileState;
        }

        public static List<TileState> GetAllTileStates()
        {
            List<TileState> tileStates = new List<TileState>();

            for (int i = 0; i < TileContainer.Children.Count; i++)
            {
                TileControl currentTile = (TileControl)TileContainer.Children[i];
                tileStates.Add(GetTileState(currentTile));
            }

            for (int i = 0; i < PinnedTileContainer.Children.Count; i++)
            {
                TileControl currentTile = (TileControl)PinnedTileContainer.Children[i];
                tileStates.Add(GetTileState(currentTile));
            }

            return tileStates;
        }

        public static void LoadTiles()
        {
            string libraryPath = App.Settings.path + @"\Library";
            if (Directory.Exists(libraryPath))
            {
                foreach (string dir in Directory.GetDirectories(libraryPath))
                {
                    string path = string.Format(@"{0}\{1}.dll", dir, Path.GetFileName(dir));
                    if (File.Exists(path))
                    {
                        Tile currentTile = new Tile(path);
                        if (!currentTile.HasErrors)
                        {
                            Tiles.Add(currentTile);
                        }

                        // TODO: Decide if it is Tile Manager's job to populate the add tile menu
                        /*Tiles.Add(new Tile(path));
                        if (Tiles[Tiles.Count - 1].hasErrors)
                            Tiles.RemoveAt(Tiles.Count - 1);
                        else
                        {
                            MenuItem item = new MenuItem();
                            if (Tiles[Tiles.Count - 1].Info != null)
                                item.Header = Tiles[Tiles.Count - 1].Info.Name;
                            item.Click += new RoutedEventHandler(AddTileSubItem_Click);
                            Image icon = new Image();
                            icon.Source = Tiles[Tiles.Count - 1].TitleIcon.Source;
                            icon.Width = 25;
                            icon.Height = 25;
                            item.Icon = icon;
                            AddTileItem.Items.Add(item);
                        }*/
                    }
                }
            }
        }

        public static void LoadTileList(List<TileState> tileList, bool clearList)
        {
            if (clearList)
            {
                TileContainer.Children.Clear();
                PinnedTileContainer.Children.Clear();
            }

            tileList.Sort((x, y) => x.Order.CompareTo(y.Order));

            for (int i = 0; i < tileList.Count; i++)
            {
                TileState state = tileList[i];
                foreach (Tile tile in Tiles)
                {
                    if (Path.GetFileName(tile.Path) == state.Name && !tile.HasErrors)
                    {
                        TileControl control = tile.CreateControl(state);

                        if (control.hasErrors)
                        {
                            return;
                        }

                        if (state.IsPinned)
                        {
                            PinnedTileContainer.Children.Add(control);
                        }
                        else
                        {
                            TileContainer.Children.Add(control);
                        }
                    }
                }
            }
        }

        public static void LoadTileList(List<TileState> tiles)
        {
            LoadTileList(tiles, true);
        }
    }
}
