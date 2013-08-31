using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using Microsoft.Phone.Marketplace;
using Mono.Api.ReactivePinboard;
using Mono.Api.ReactivePinboard.Model;
using Mono.App.Pinboard.Helper;
using Mono.App.Pinboard.Model;
using Mono.App.Pinboard.ViewModel;
using Mono.Framework.Common.Extensions;

namespace Mono.App.Pinboard.Service
{
    internal class PinboardService
    {
        private PinboardService()
        {
            IsTrial = new LicenseInformation().IsTrial();
            allItems = new List<BookmarkItemViewModel>();
            unreadItems = new List<BookmarkItemViewModel>();
            privateItems = new List<BookmarkItemViewModel>();
            publicItems = new List<BookmarkItemViewModel>();
            untaggedItems = new List<BookmarkItemViewModel>();
            taggedItems = new List<BookmarkItemViewModel>();
            networkItems = new List<FeedItemViewModel>();
            popularItems = new List<FeedItemViewModel>();
            recentItems = new List<FeedItemViewModel>();
        }

        private PinboardClient client;
        private static object lockObject = new object();

        private static PinboardService service = new PinboardService();

        public bool IsLoaded
        {
            get
            {
                return isAllLoaded && isPopularLoaded && isNetworkLoaded && isRecentLoaded && isTagLoaded;
            }
        }

        public bool IsLoadedOnlyNoLogin
        {
            get
            {
                return isPopularLoaded && isRecentLoaded;
            }
        }

        private bool isAllLoaded;
        private bool isPopularLoaded;
        private bool isNetworkLoaded;
        private bool isRecentLoaded;
        private bool isTagLoaded;

        public IList<BookmarkItemViewModel> allItems { get; private set; }// = new List<BookmarkItemViewModel>();

        public IList<BookmarkItemViewModel> unreadItems { get; private set; }//= new List<BookmarkItemViewModel>();

        public IList<BookmarkItemViewModel> privateItems { get; private set; }// = new List<BookmarkItemViewModel>();

        public IList<BookmarkItemViewModel> publicItems { get; private set; }//= new List<BookmarkItemViewModel>();

        public IList<BookmarkItemViewModel> untaggedItems { get; private set; }//= new List<BookmarkItemViewModel>();

        public IList<BookmarkItemViewModel> taggedItems { get; private set; }// { get; private set; }//= new List<BookmarkItemViewModel>();

        public IList<FeedItemViewModel> networkItems { get; private set; }// = new List<FeedItemViewModel>();

        public IList<FeedItemViewModel> popularItems { get; private set; }// = new List<FeedItemViewModel>();

        public IList<FeedItemViewModel> recentItems { get; private set; }// = new List<FeedItemViewModel>();

        public void CreateLocalDatabaseIfNotExist()
        {
            using (var context = new AppContext())
            {
                if (!context.DatabaseExists())
                {
                    context.CreateDatabase();
                }
            }
        }

        public bool IsTrial { get; private set; }

        public static PinboardService GetInstance()
        {
            if (service.client == null)
            {
                service.client = new PinboardClient(SettingsService.LoadSettingOrDefault<string>("account"),
                                                    SettingsService.LoadSettingOrDefault<string>("password"));
            }

            return service;
        }

        public IObservable<bool> IsLogin()
        {
            if (!SettingsService.Contains(Consts.AccountKey) || !SettingsService.Contains(Consts.PasswordKey))
            {
                return new BehaviorSubject<bool>(false);
            }

            client = new PinboardClient(SettingsService.LoadSettingOrDefault<string>(Consts.AccountKey),
                                        SettingsService.LoadSettingOrDefault<string>(Consts.PasswordKey));
            return client.GetUserSecret()
                .Select(s =>
                {
                    if (string.IsNullOrEmpty(s))
                    {
                        return false;
                    }
                    else
                    {
                        SettingsService.SaveSetting(Consts.SecretKey, s);
                        return true;
                    }
                });
        }

        public IObservable<bool> Login(string account, string password)
        {
            client = new PinboardClient(account, password);
            return client.GetUserSecret()
                .Select(secret =>
                {
                    if (string.IsNullOrEmpty(secret))
                    {
                        return false;
                    }
                    else
                    {
                        SettingsService.SaveAccountInfo(account, password, secret);
                        return true;
                    }
                });
        }

        public void AddTagsSource(string tag)
        {
            LockAndUsingDB(context =>
            {
                context.Tags.InsertOnSubmit(new Tag { Name = tag, Count = 1 });
                context.SubmitChanges();
            });
        }

        public IObservable<string> DeleteBookmark(string uri)
        {
            return Observable.Defer(() =>
                {
                    DeleteBookmarkDB(uri);
                    return client.DeletePost(uri);
                });
        }

        private static void DeleteBookmarkDB(string uri)
        {
            LockAndUsingDB(context =>
            {
                var target = context.Items.OfType<Post>().Where(x => x.Url == uri).FirstOrDefault();
                if (target != null)
                {
                    context.Items.DeleteOnSubmit(target);
                    context.SubmitChanges();
                }
            });
        }

        public IObservable<string> SaveBookmark(BookmarkItemViewModel vm)
        {
            var post = new Post
                {
                    Url = vm.Url,
                    Title = vm.Title,
                    Description = vm.Description,
                    Shared = !vm.IsPrivate,
                    Tags = vm.Tags.ToArray(),
                    Time = DateTime.Now,
                    ToReade = vm.IsUnread,
                };
            LockAndUsingDB(context =>
            {
                var target = context.Items.OfType<Post>().Where(x => x.Url == post.Url).FirstOrDefault();
                if (target == null)
                {
                    post.Id = Guid.NewGuid().ToString();
                    context.Items.InsertOnSubmit(post);
                }
                else
                {
                    target.Title = vm.Title;
                    target.Description = vm.Description;
                    target.Shared = vm.IsPrivate;
                    target.Tags = vm.Tags.ToArray();
                    target.Time = DateTime.Now;
                    target.ToReade = vm.IsUnread;
                }
                context.SubmitChanges();
            });

            return client.AddPost(post);
        }

        public IObservable<string> DeleteBookmark(BookmarkItemViewModel vm)
        {
            DeleteBookmarkDB(vm.Url);
            return client.DeletePost(vm.Url);
        }

        public IObservable<string> RenameTag(string oldName, string newName)
        {
            LockAndUsingDB(context =>
            {
                var target = context.Tags.Where(x => x.Name == oldName).FirstOrDefault();
                if (target != null)
                {
                    var tag = new Tag { Name = newName, Count = target.Count };
                    context.Tags.DeleteOnSubmit(target);
                    context.Tags.InsertOnSubmit(tag);

                    var items = context.Items.ToList().Where(x => x.Tags.Contains(oldName)).Select(x => x.Url).ToArray();
                    foreach (var x in context.Items.Where(x => items.Contains(x.Url)))
                    {
                        var n = x.Tags.ToList().IndexOf(oldName);
                        x.Tags[n] = newName;
                    }

                    context.SubmitChanges();
                }
            });
            return client.RenameTag(oldName, newName);
        }

        public IObservable<IEnumerable<Post>> GetAllBookmarks()
        {
            isAllLoaded = false;
            return client.Update().SelectMany(lastModified =>
                {
                    //last sync以降に更新があったとき
                    var lastSync = SettingsService.LoadSettingOrDefault<DateTime?>(Consts.LastSynkKey);
                    if (!lastSync.HasValue || lastSync.Value.CompareTo(lastModified) < 0)
                    {
                        SettingsService.SaveSetting(Consts.LastSynkKey, DateTime.Now);
                        return client.GetAllBookmarks()
                        .Select(posts =>
                        {
                            isAllLoaded = true;
                            var ordered = posts.OrderByDescending(x => x.Time);
                            RefreshPostItems(ordered);
                            return SaveAndReturnItem(ordered);
                        });
                    }
                    else
                    {
                        isAllLoaded = true;
                        return Observable.Empty<IEnumerable<Post>>();
                    }
                });
        }

        public static void LockAndUsingDB(Action<AppContext> action, bool trackingEnabled = true)
        {
            lock (lockObject)
            {
                using (var context = new AppContext { ObjectTrackingEnabled = trackingEnabled })
                {
                    action(context);
                }
            }
        }

        public void LoadPostFromDB(Action action)
        {
            PinboardService.LockAndUsingDB(context =>
            {
                RefreshPostItems(context.Items.OfType<Post>());
                RefreshFeedItems(context.Items.OfType<FeedItem>(), true);
            }, false);
            if (action != null)
            {
                action();
            }
        }

        private void RefreshPostItems<T>(IEnumerable<T> posts) where T : Post
        {
            allItems.Clear();
            unreadItems.Clear();
            privateItems.Clear();
            publicItems.Clear();
            untaggedItems.Clear();
            taggedItems.Clear();
            posts.ForEach(p =>
                {
                    var vm = p.ToItemViewModel();
                    allItems.Add(vm);
                    if (vm.IsUnread)
                    {
                        unreadItems.Add(vm);
                    }
                    if (vm.IsPrivate)
                    {
                        privateItems.Add(vm);
                    }
                    else
                    {
                        publicItems.Add(vm);
                    }
                    if (vm.Tags.Count == 0)
                    {
                        untaggedItems.Add(vm);
                    }
                    else
                    {
                        taggedItems.Add(vm);
                    }
                });
        }

        private void RefreshFeedItems<T>(IEnumerable<T> feeds, bool refreshAll = false) where T : FeedItem
        {
            if (refreshAll)
            {
                networkItems.Clear();
                popularItems.Clear();
                recentItems.Clear();
            }
            feeds.ForEach(feed =>
                    {
                        if (feed is NetworkItem)
                        {
                            networkItems.Add(feed.ToItemViewModel());
                        }
                        else if (feed is PopularItem)
                        {
                            popularItems.Add(feed.ToItemViewModel());
                        }
                        else if (feed is RecentItem)
                        {
                            recentItems.Add(feed.ToItemViewModel());
                        }
                    });
        }

        private static IEnumerable<T> SaveAndReturnItem<T>(IEnumerable<T> items)
            where T : ItemBase
        {
            LockAndUsingDB(context =>
            {
                var old = context.Items.OfType<T>();
                context.Items.DeleteAllOnSubmit(old);
                context.Items.InsertAllOnSubmit(items);
                context.SubmitChanges();
            });
            return items;
        }

        public IObservable<IEnumerable<NetworkItem>> GetNetwork()
        {
            isNetworkLoaded = false;
            if (SettingsService.Contains(Consts.SecretKey))
            {
                return client.GetNetwork(SettingsService.LoadSettingOrDefault<string>(Consts.SecretKey)).Select(items =>
                {
                    if (items == null || items.Count() == 0)
                    {
                        return items;
                    }
                    isNetworkLoaded = true;
                    networkItems.Clear();
                    RefreshFeedItems(items);
                    return SaveAndReturnItem(items);
                });
            }
            return client.GetUserSecret().SelectMany(s =>
            {
                if (string.IsNullOrEmpty(s))
                {
                    return Observable.Empty<IEnumerable<NetworkItem>>();
                }
                SettingsService.SaveSetting(Consts.SecretKey, s);
                return client.GetNetwork(s).Select(items =>
                {
                    if (items == null || items.Count() == 0)
                    {
                        return items;
                    }
                    isNetworkLoaded = true;
                    networkItems.Clear();
                    RefreshFeedItems(items);
                    return SaveAndReturnItem(items);
                });
            });
        }

        public IObservable<IEnumerable<PopularItem>> GetPopular()
        {
            isPopularLoaded = false;
            return client.GetPopular().Select(items =>
            {
                if (items == null || items.Count() == 0)
                {
                    return items;
                }
                isPopularLoaded = true;
                popularItems.Clear();
                RefreshFeedItems(items);
                return SaveAndReturnItem(items);
            });
        }

        public IObservable<IEnumerable<RecentItem>> GetRecentAll()
        {
            isRecentLoaded = false;

            return client.GetRecentAll().Select(items =>
            {
                if (items == null || items.Count() == 0)
                {
                    return items;
                }
                isRecentLoaded = true;
                recentItems.Clear();
                RefreshFeedItems(items);
                return SaveAndReturnItem(items);
            });
        }

        public IObservable<IEnumerable<FeedItem>> GetUserItems(string account)
        {
            return client.GetUserFeed(account);
        }

        public IObservable<IEnumerable<FeedItem>> GetTagItems(string tag)
        {
            return client.GetTagFeed(tag);
        }

        public IObservable<IEnumerable<Tag>> GetTags()
        {
            isTagLoaded = false;

            return client.GetTags().Select(x =>
                {
                    isTagLoaded = true;
                    LockAndUsingDB(context =>
                    {
                        var old = context.Tags;
                        context.Tags.DeleteAllOnSubmit(old);
                        context.Tags.InsertAllOnSubmit(x);
                        context.SubmitChanges();
                    });
                    return x;
                });
        }

        public IList<Tag> GetAllAccounts()
        {
            var result = new List<Tag>();
            LockAndUsingDB(context =>
            {
                var accounts = context.Items.OfType<FeedItem>().Select(x => x.Author).GroupBy(x => x);
                foreach (var s in accounts)
                {
                    var u = new Tag { Name = s.Key, Count = s.Count() };
                    result.Add(u);
                }
            }, false);
            return result;
        }

        public IList<Tag> GetAllTags()
        {
            var result = new List<Tag>();
            LockAndUsingDB(context =>
            {
                var tagss = context.Items.Select(x => x.Tags).ToArray();

                foreach (var s in tagss.SelectMany(x => x).GroupBy(x => x))
                {
                    var u = new Tag { Name = s.Key, Count = s.Count() };
                    result.Add(u);
                }
            }, false);
            return result;
        }
    }
}