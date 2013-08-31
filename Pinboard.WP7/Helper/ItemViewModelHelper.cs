using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Mono.Api.ReactivePinboard.Model;
using Mono.App.Pinboard.ViewModel;

namespace Mono.App.Pinboard.Helper
{
    internal static class ItemViewModelHelper
    {
        public static BookmarkItemViewModel ToBookmark(this FeedItemViewModel feed)
        {
            return new BookmarkItemViewModel
            {
                Description = feed.Description,
                IsPrivate = false,
                IsUnread = false,
                Tags = feed.Tags,
                Time = DateTime.Now,
                Title = feed.Title,
                Url = feed.Url,
            };
        }

        public static FeedItemViewModel ToItemViewModel(this FeedItem feed)
        {
            var tags = new ObservableCollection<string>();
            foreach (var t in feed.Tags)
            {
                tags.Add(t);
            }
            return new FeedItemViewModel
            {
                Url = feed.Url,
                Title = feed.Title,
                Description = feed.Description,
                Tags = tags,
                Time = feed.Time.HasValue ? feed.Time.Value : DateTime.Now,
                Identifier = feed.Id,
                Author = feed.Author,
            };
        }

        public static BookmarkItemViewModel ToItemViewModel(this Post post)
        {
            var tags = new ObservableCollection<string>();
            foreach (var t in post.Tags)
            {
                tags.Add(t);
            }
            var isPrivate = false;
            if (post.Shared.HasValue && post.Shared.Value == false)
            {
                isPrivate = true;
            }
            var isToread = false;
            if (post.ToReade.HasValue && post.ToReade.Value)
            {
                isToread = true;
            }
            return new BookmarkItemViewModel
            {
                Url = post.Url,
                Title = post.Title,
                Description = post.Description,
                Tags = tags,
                Time = post.Time.HasValue ? post.Time.Value : DateTime.Now,

                IsPrivate = isPrivate,
                IsUnread = isToread,
                Hash = post.Id,
                Meta = post.Meta,
            };
        }
    }
}