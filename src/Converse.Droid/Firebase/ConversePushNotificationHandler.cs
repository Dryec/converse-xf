using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Graphics;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.FirebasePushNotification.Abstractions;
using Android.Media;
using Android.Support.V4.App;
using System.Collections.ObjectModel;
using Android.Content.PM;
using Android.Content.Res;
using Java.Util;
using Plugin.FirebasePushNotification;
using Converse.Helpers;
using Newtonsoft.Json;
using Converse.Models;
using Converse.Tron;
using Converse.Database;

namespace Converse.Droid.Firebase
{
    public class ConversePushNotificationHandler : IPushNotificationHandler
    {
        public const string DomainTag = "ConversePushNotificationHandler";

        public void OnError(string error)
        {
            System.Diagnostics.Debug.WriteLine($"{DomainTag} - OnError - {error}");
        }

        public void OnOpened(NotificationResponse response)
        {
            System.Diagnostics.Debug.WriteLine($"{DomainTag} - OnOpened");
        }

        public async void OnReceived(IDictionary<string, object> parameters)
        {
            System.Diagnostics.Debug.WriteLine($"{DomainTag} - OnReceived");

            if ((parameters.TryGetValue(SilentKey, out var silent) && (silent.ToString() == "true" || silent.ToString() == "1")))
                return;

            var context = Application.Context;

            var notifyId = 0;
            var title = context.ApplicationInfo.LoadLabel(context.PackageManager);
            string message = null;
            var tag = string.Empty;
            string type = null;

            if (parameters.TryGetValue(IdKey, out var id))
            {
                try
                {
                    notifyId = Convert.ToInt32(id);
                }
                catch (Exception ex)
                {
                    // Keep the default value of zero for the notify_id, but log the conversion problem.
                    System.Diagnostics.Debug.WriteLine($"Failed to convert {id} to an integer {ex}");
                }
            }

            if (parameters.TryGetValue(TypeKey, out var typeStr))
            {
                type = $"{typeStr}";

                switch (type)
                {
                    case AppConstants.FCM.Types.Message:
                    case AppConstants.FCM.Types.GroupMessage:
                        if (parameters.TryGetValue(DataKey, out var data))
                        {
                            try
                            {
                                var chatMessage = JsonConvert.DeserializeObject<ChatMessage>($"{data}");
                                var walletManager = (WalletManager)App.Current.Container.Resolve(typeof(WalletManager));

                                if(chatMessage.Sender.TronAddress == walletManager.Wallet.Address)
                                {
                                    return;
                                }

                                chatMessage.Decrypt(walletManager.Wallet, chatMessage.Sender.PublicKey);

                                if(chatMessage.ExtendedMessage == null)
                                {
                                    var database = (ConverseDatabase)App.Current.Container.Resolve(typeof(ConverseDatabase));
                                    var dbChat = await database.Chats.GetByChatID(notifyId);
                                    if(dbChat != null)
                                    {
                                        var chat = dbChat.ToChatEntry();
                                        if(chat.Type == Enums.ChatType.Group)
                                        {
                                            var privKey = chat.GroupInfo.IsPublic ? chat.GroupInfo.PrivateKey : walletManager.Wallet.Decrypt(chat.GroupInfo.PrivateKey, chat.GroupInfo.PublicKey);
                                            chatMessage.Decrypt(privKey, chatMessage.Sender.PublicKey);
                                        }
                                    }
                                }

                                message = chatMessage.ExtendedMessage.Message;
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine(ex);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }


            if (message == null)
            {
                if (!string.IsNullOrEmpty(FirebasePushNotificationManager.NotificationContentTextKey) && parameters.TryGetValue(FirebasePushNotificationManager.NotificationContentTextKey, out var notificationContentText))
                    message = notificationContentText.ToString();
                else if (parameters.TryGetValue(AlertKey, out var alert))
                    message = $"{alert}";
                else if (parameters.TryGetValue(BodyKey, out var body))
                    message = $"{body}";
                else if (parameters.TryGetValue(MessageKey, out var messageContent))
                    message = $"{messageContent}";
                else if (parameters.TryGetValue(SubtitleKey, out var subtitle))
                    message = $"{subtitle}";
                else if (parameters.TryGetValue(TextKey, out var text))
                    message = $"{text}";
                else
                    message = string.Empty;
            }

            if (!string.IsNullOrEmpty(FirebasePushNotificationManager.NotificationContentTitleKey) && parameters.TryGetValue(FirebasePushNotificationManager.NotificationContentTitleKey, out var notificationContentTitle))
                title = notificationContentTitle.ToString();
            else if (parameters.TryGetValue(TitleKey, out var titleContent))
            {
                if (!string.IsNullOrEmpty(message))
                    title = $"{titleContent}";
                else
                    message = $"{titleContent}";
            }


            if (parameters.TryGetValue(TagKey, out var tagContent))
                tag = tagContent.ToString();

            try
            {
                if (parameters.TryGetValue(SoundKey, out var sound))
                {
                    var soundName = sound.ToString();

                    var soundResId = context.Resources.GetIdentifier(soundName, "raw", context.PackageName);
                    if (soundResId == 0 && soundName.IndexOf(".", StringComparison.Ordinal) != -1)
                    {
                        soundName = soundName.Substring(0, soundName.LastIndexOf('.'));
                        soundResId = context.Resources.GetIdentifier(soundName, "raw", context.PackageName);
                    }

                    FirebasePushNotificationManager.SoundUri = new Android.Net.Uri.Builder()
                                .Scheme(ContentResolver.SchemeAndroidResource)
                                .Path($"{context.PackageName}/{soundResId}")
                                .Build();
                }
            }
            catch (Android.Content.Res.Resources.NotFoundException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            if (FirebasePushNotificationManager.SoundUri == null)
                FirebasePushNotificationManager.SoundUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);

            try
            {
                if (parameters.TryGetValue(IconKey, out var icon) && icon != null)
                {
                    try
                    {
                        FirebasePushNotificationManager.IconResource = context.Resources.GetIdentifier(icon.ToString(), "drawable", Application.Context.PackageName);
                    }
                    catch (Android.Content.Res.Resources.NotFoundException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }

                if (FirebasePushNotificationManager.IconResource == 0)
                    FirebasePushNotificationManager.IconResource = context.ApplicationInfo.Icon;
                else
                {
                    var name = context.Resources.GetResourceName(FirebasePushNotificationManager.IconResource);
                    if (name == null)
                        FirebasePushNotificationManager.IconResource = context.ApplicationInfo.Icon;
                }
            }
            catch (Android.Content.Res.Resources.NotFoundException ex)
            {
                FirebasePushNotificationManager.IconResource = context.ApplicationInfo.Icon;
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            if (parameters.TryGetValue(ColorKey, out var color) && color != null)
            {
                try
                {
                    FirebasePushNotificationManager.Color = Color.ParseColor(color.ToString());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"{DomainTag} - Failed to parse color {ex}");
                }
            }

            var resultIntent = typeof(Activity).IsAssignableFrom(FirebasePushNotificationManager.NotificationActivityType) ? new Intent(Application.Context, FirebasePushNotificationManager.NotificationActivityType) : context.PackageManager.GetLaunchIntentForPackage(context.PackageName);

            var extras = new Bundle();
            foreach (var p in parameters)
                extras.PutString(p.Key, p.Value.ToString());

            if (extras != null)
            {
                extras.PutInt(ActionNotificationIdKey, notifyId);
                extras.PutString(ActionNotificationTagKey, tag);
                resultIntent.PutExtras(extras);
            }

            if (FirebasePushNotificationManager.NotificationActivityFlags != null)
            {
                resultIntent.SetFlags(FirebasePushNotificationManager.NotificationActivityFlags.Value);
            }
            var requestCode = new Java.Util.Random().NextInt();

            var pendingIntent = PendingIntent.GetActivity(context, requestCode, resultIntent, PendingIntentFlags.UpdateCurrent);

            var chanId = FirebasePushNotificationManager.DefaultNotificationChannelId;
            if (parameters.TryGetValue(ChannelIdKey, out var channelId) && channelId != null)
            {
                chanId = $"{channelId}";
            }

            var notificationBuilder = new NotificationCompat.Builder(context, chanId)
                 .SetSmallIcon(FirebasePushNotificationManager.IconResource)
                 .SetContentTitle(title)
                 .SetContentText(message)
                 .SetAutoCancel(true)
                 .SetContentIntent(pendingIntent);

            var deleteIntent = new Intent(context, typeof(PushNotificationDeletedReceiver));
            var pendingDeleteIntent = PendingIntent.GetBroadcast(context, requestCode, deleteIntent, PendingIntentFlags.CancelCurrent);
            notificationBuilder.SetDeleteIntent(pendingDeleteIntent);

            if (Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.O)
            {
                if (parameters.TryGetValue(PriorityKey, out var priority) && priority != null)
                {
                    var priorityValue = $"{priority}";
                    if (!string.IsNullOrEmpty(priorityValue))
                    {
                        switch (priorityValue.ToLower())
                        {
                            case "max":
                                notificationBuilder.SetPriority((int)Android.App.NotificationPriority.Max);
                                notificationBuilder.SetVibrate(new long[] { 1000, 1000, 1000, 1000, 1000 });
                                break;
                            case "high":
                                notificationBuilder.SetPriority((int)Android.App.NotificationPriority.High);
                                notificationBuilder.SetVibrate(new long[] { 1000, 1000, 1000, 1000, 1000 });
                                break;
                            case "default":
                                notificationBuilder.SetPriority((int)Android.App.NotificationPriority.Default);
                                notificationBuilder.SetVibrate(new long[] { 1000, 1000, 1000, 1000, 1000 });
                                break;
                            case "low":
                                notificationBuilder.SetPriority((int)Android.App.NotificationPriority.Low);
                                break;
                            case "min":
                                notificationBuilder.SetPriority((int)Android.App.NotificationPriority.Min);
                                break;
                            default:
                                notificationBuilder.SetPriority((int)Android.App.NotificationPriority.Default);
                                notificationBuilder.SetVibrate(new long[] { 1000, 1000, 1000, 1000, 1000 });
                                break;
                        }

                    }
                    else
                    {
                        notificationBuilder.SetVibrate(new long[] { 1000, 1000, 1000, 1000, 1000 });
                    }

                }
                else
                {
                    notificationBuilder.SetVibrate(new long[] { 1000, 1000, 1000, 1000, 1000 });
                }

                try
                {

                    notificationBuilder.SetSound(FirebasePushNotificationManager.SoundUri);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"{DomainTag} - Failed to set sound {ex}");
                }
            }


            if (FirebasePushNotificationManager.Color != null)
                notificationBuilder.SetColor(FirebasePushNotificationManager.Color.Value);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
            {
                // Using BigText notification style to support long message
                var style = new NotificationCompat.BigTextStyle();
                style.BigText(message);
                notificationBuilder.SetStyle(style);
            }

            var category = string.Empty;
            if (parameters.TryGetValue(CategoryKey, out var categoryContent))
                category = categoryContent.ToString();

            if (parameters.TryGetValue(ActionKey, out var actionContent))
                category = actionContent.ToString();

            var notificationCategories = CrossFirebasePushNotification.Current?.GetUserNotificationCategories();
            if (notificationCategories != null && notificationCategories.Length > 0)
            {
                IntentFilter intentFilter = null;
                foreach (var userCat in notificationCategories)
                {
                    if (userCat != null && userCat.Actions != null && userCat.Actions.Count > 0)
                    {
                        foreach (var action in userCat.Actions)
                        {
                            var aRequestCode = Guid.NewGuid().GetHashCode();

                            if (userCat.Category.Equals(category, StringComparison.CurrentCultureIgnoreCase))
                            {
                                Intent actionIntent = null;
                                PendingIntent pendingActionIntent = null;


                                if (action.Type == NotificationActionType.Foreground)
                                {
                                    actionIntent = typeof(Activity).IsAssignableFrom(FirebasePushNotificationManager.NotificationActivityType) ? new Intent(Application.Context, FirebasePushNotificationManager.NotificationActivityType) : context.PackageManager.GetLaunchIntentForPackage(context.PackageName);

                                    if (FirebasePushNotificationManager.NotificationActivityFlags != null)
                                    {
                                        actionIntent.SetFlags(FirebasePushNotificationManager.NotificationActivityFlags.Value);
                                    }

                                    extras.PutString(ActionIdentifierKey, action.Id);
                                    actionIntent.PutExtras(extras);
                                    pendingActionIntent = PendingIntent.GetActivity(context, aRequestCode, actionIntent, PendingIntentFlags.UpdateCurrent);

                                }
                                else
                                {
                                    actionIntent = new Intent(context, typeof(PushNotificationActionReceiver));
                                    extras.PutString(ActionIdentifierKey, action.Id);
                                    actionIntent.PutExtras(extras);
                                    pendingActionIntent = PendingIntent.GetBroadcast(context, aRequestCode, actionIntent, PendingIntentFlags.UpdateCurrent);

                                }

                                notificationBuilder.AddAction(new NotificationCompat.Action.Builder(context.Resources.GetIdentifier(action.Icon, "drawable", Application.Context.PackageName), action.Title, pendingActionIntent).Build());
                            }

                        }
                    }
                }


            }

            OnBuildNotification(notificationBuilder, parameters);

            var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            notificationManager.Notify(tag, notifyId, notificationBuilder.Build());
        }

        /// <summary>
        /// Override to provide customization of the notification to build.
        /// </summary>
        /// <param name="notificationBuilder">Notification builder.</param>
        /// <param name="parameters">Notification parameters.</param>
        public virtual void OnBuildNotification(NotificationCompat.Builder notificationBuilder, IDictionary<string, object> parameters) { }

        /// <summary>
        /// Data
        /// </summary>
        public const string DataKey = "data";
        /// <summary>
        /// Type
        /// </summary>
        public const string TypeKey = "type";
        /// <summary>
        /// Title
        /// </summary>
        public const string TitleKey = "title";
        /// <summary>
        /// Text
        /// </summary>
        public const string TextKey = "text";
        /// <summary>
        /// Subtitle
        /// </summary>
        public const string SubtitleKey = "subtitle";
        /// <summary>
        /// Message
        /// </summary>
        public const string MessageKey = "message";
        /// <summary>
        /// Message
        /// </summary>
        public const string BodyKey = "body";
        /// <summary>
        /// Alert
        /// </summary>
        public const string AlertKey = "alert";

        /// <summary>
        /// Id
        /// </summary>
        public const string IdKey = "id";

        /// <summary>
        /// Tag
        /// </summary>
        public const string TagKey = "tag";

        /// <summary>
        /// Action Click
        /// </summary>
        public const string ActionKey = "click_action";

        /// <summary>
        /// Category
        /// </summary>
        public const string CategoryKey = "category";

        /// <summary>
        /// Silent
        /// </summary>
        public const string SilentKey = "silent";

        /// <summary>
        /// ActionNotificationId
        /// </summary>
        public const string ActionNotificationIdKey = "action_notification_id";

        /// <summary>
        /// ActionNotificationTag
        /// </summary>
        public const string ActionNotificationTagKey = "action_notification_tag";

        /// <summary>
        /// ActionIdentifier
        /// </summary>
        public const string ActionIdentifierKey = "action_identifier";

        /// <summary>
        /// Color
        /// </summary>
        public const string ColorKey = "color";

        /// <summary>
        /// Icon
        /// </summary>
        public const string IconKey = "icon";

        /// <summary>
        /// Sound
        /// </summary>
        public const string SoundKey = "sound";


        /// <summary>
        /// Priority
        /// </summary>
        public const string PriorityKey = "priority";

        /// <summary>
        /// Channel id
        /// </summary>
        public const string ChannelIdKey = "android_channel_id";
    }
}
