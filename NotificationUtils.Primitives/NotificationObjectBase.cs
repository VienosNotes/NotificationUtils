using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace NotificationUtils.Primitives
 {
     /// <summary>
     /// 変更通知オブジェクトの基本的な機能を提供します。
     /// </summary>
     public class NotificationObjectBase : INotifyPropertyChanged
     {
         /// <inheritdoc/>>
         public event PropertyChangedEventHandler PropertyChanged;
         
         /// <summary>
         /// プロパティへのセットを行う動的コードのキャッシュ。
         /// </summary>
         private static readonly Dictionary<DynamicPropertySetKey, Action<object, object>> InvokerCache = new Dictionary<DynamicPropertySetKey, Action<object, object>>();
         
         /// <summary>
         /// 対象のプロパティに値を設定し、PropertyChanged イベントを発火します。
         /// </summary>
         /// <param name="propName">対象のプロパティ名。</param>
         /// <param name="value">設定する値。</param>
         /// <typeparam name="T">対象のプロパティの型。</typeparam>
         public virtual void SetProperty<T>(string propName, T value)
         {
             var key = new DynamicPropertySetKey
             {
                 Type = GetType(), // 実行時の派生先の型
                 Property = propName
             };

             // キャッシュに生成済みの動的コードがあれば使用し、なければ作ってキャッシュに追加する
             if (!InvokerCache.TryGetValue(key, out var invoker))
             {
                 // セットされる新しい値
                 var p = Expression.Parameter(typeof(object));
                 // 対象のプロパティのオーナー
                 var self = Expression.Parameter(typeof(object));
                 
                 var exp = Expression.Lambda<Action<object, object>>(
                     Expression.Block(
                         Expression.Assign(
                             Expression.Property(
                                 Expression.Convert(self, key.Type), 
                                 propName),
                             Expression.Convert(p, typeof(T))
                             )
                         ), self, p);
                 invoker = exp.Compile();
                 InvokerCache[key] = invoker;
             }
             
             invoker.Invoke(this, value);
             RaisePropertyChanged(propName);
         }
         
         /// <summary>
         /// PropertyChanged イベントを発火させます。 
         /// </summary>
         /// <param name="propName">プロパティ名。</param>
         protected virtual void RaisePropertyChanged(string propName)
         {
             PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
         }
         
         /// <summary>
         /// プロパティセットを行う動的コードを区別するためのキー。
         /// </summary>
         private class DynamicPropertySetKey
         {
             /// <summary>
             /// プロパティのオーナーの型。
             /// </summary>
             public Type Type;
             /// <summary>
             /// プロパティ名。
             /// </summary>
             public string Property;
             
             /// <inheritdoc/>
             public override int GetHashCode()
             {
                 return Type.GetHashCode() ^ Property.GetHashCode();
             }
         }
     }
 }