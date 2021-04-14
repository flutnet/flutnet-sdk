// Copyright (c) 2020-2021 Novagem Solutions S.r.l.
//
// This file is part of Flutnet.
//
// Flutnet is a free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Flutnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with Flutnet.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Media;
using ReactiveUI;

namespace FlutnetUI.ViewModels
{
    public abstract class TreeItem : ReactiveObject
    {
        protected TreeItem(string text, IEnumerable<TreeItem> children = null)
        {
            Text = text;

            Children = new ObservableCollection<TreeItem>();
            if (children != null)
            {
                foreach (TreeItem child in children)
                {
                    AddChild(child);
                }
            }
        }

        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }
        string _text;

        public Drawing Image
        {
            get => _image;
            set => this.RaiseAndSetIfChanged(ref _image, value);
        }
        Drawing _image;

        public bool IsExpanded
        {
            get => true; //NOTE: Quick fix for prevent an item to be collapsed, should return _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }
        bool _isExpanded = true;

        public bool IsSelected
        {
            get => false; //NOTE: Quick fix for prevent an item to be selected, should return _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }
        bool _isSelected;

        public TreeItem Parent
        {
            get => _parent;
            private set => this.RaiseAndSetIfChanged(ref _parent, value);
        }
        TreeItem _parent;

        public void ExpandPath()
        {
            IsExpanded = true;
            Parent?.ExpandPath();
        }

        public void CollapsePath()
        {
            IsExpanded = false;
            Parent?.CollapsePath();
        }

        public ObservableCollection<TreeItem> Children { get; }

        public void AddChild(TreeItem child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            child.Parent = this;
            Children.Add(child);
        }

        public void RemoveChild(TreeItem child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            child.Parent = null;
            Children.Remove(child);
        }

        public override string ToString()
        {
            return Text;
        }
    }

    public class FolderTreeItem : TreeItem
    {
        public FolderTreeItem(string name, IEnumerable<TreeItem> children = null)
            : base(name, children)
        {
        }
    }

    public class FileTreeItem : TreeItem
    {
        public FileTreeItem(string name) : base(name)
        {
        }
    }
}