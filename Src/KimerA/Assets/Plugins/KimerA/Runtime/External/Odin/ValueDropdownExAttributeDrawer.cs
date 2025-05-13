#if UNITY_EDITOR && ODIN_INSPECTOR

#pragma warning disable CS8603 // 可能返回 null 引用。

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.ActionResolvers;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
    public sealed class ValueDropdownExAttributeDrawer : OdinAttributeDrawer<ValueDropdownExAttribute>
    {

        private string? error;

        private GUIContent? label;

        private bool isList;

        private bool isListElement;

        private Func<IEnumerable<ValueDropdownItem>>? getValues;

        private Func<IEnumerable<object>>? getSelection;

        private IEnumerable<object>? result;

        private bool enableMultiSelect;

        private Dictionary<object, string>? nameLookup;

        private ValueResolver<object>? rawGetter;

        private ActionResolver? itemAction;

        private LocalPersistentContext<bool>? isToggled;

        //
        // 摘要:
        //     Initializes this instance.
        protected override void Initialize()
        {
            rawGetter = ValueResolver.Get<object>(base.Property, base.Attribute.ValuesGetter);
            if (base.Attribute.ItemAction is not null)
            {
                itemAction = ActionResolver.Get(base.Property, base.Attribute.ItemAction, new[] { new ActionResolvers.NamedValue("item", typeof(OdinMenuItem)) });
            }
            isToggled = this.GetPersistentValue("Toggled", SirenixEditorGUI.ExpandFoldoutByDefault);
            error = rawGetter.ErrorMessage ?? itemAction?.ErrorMessage;
            isList = base.Property.ChildResolver is ICollectionResolver;
            isListElement = base.Property.Parent != null && base.Property.Parent.ChildResolver is ICollectionResolver;
            getSelection = () => base.Property.ValueEntry.WeakValues.Cast<object>();
            getValues = delegate
            {
                object value = rawGetter.GetValue();
                return (value != null) ? (from object x in value as IEnumerable
                                        where x != null
                                        select x).Select(delegate (object x)
                                    {
                                        if (x is ValueDropdownItem item)
                                        {
                                            return item;
                                        }

                                        if (x is IValueDropdownItem item1)
                                        {
                                            return new ValueDropdownItem(item1.GetText(), item1.GetValue());
                                        }

                                        return new ValueDropdownItem(null, x);
                                    }) : null;
            };
            ReloadDropdownCollections();
        }

        private void ReloadDropdownCollections()
        {
            if (error != null)
            {
                return;
            }

            object? obj = null;
            object? value = rawGetter?.GetValue();
            if (value != null)
            {
                obj = (value as IEnumerable).Cast<object>().FirstOrDefault();
            }

            if (obj is IValueDropdownItem)
            {
                var enumerable = getValues!.Invoke();
                nameLookup = new Dictionary<object, string>(new IValueDropdownExEqualityComparer(isTypeLookup: false));
                {
                    foreach (ValueDropdownItem item in enumerable)
                    {
                        nameLookup[item] = item.Text;
                    }

                    return;
                }
            }

            nameLookup = null;
        }

        private static IEnumerable<ValueDropdownItem> ToValueDropdowns(IEnumerable<object> query)
        {
            return query.Select(delegate (object x)
            {
                if (x is ValueDropdownItem item)
                {
                    return item;
                }

                if (x is IValueDropdownItem item1)
                {
                    return new ValueDropdownItem(item1.GetText(), item1.GetValue());
                }

                return new ValueDropdownItem(null, x);
            });
        }

        //
        // 摘要:
        //     Draws the property with GUILayout support. This method is called by DrawPropertyImplementation
        //     if the GUICallType is set to GUILayout, which is the default.
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.label = label;
            if (base.Property.ValueEntry == null)
            {
                CallNextDrawer(label);
            }
            else if (error != null)
            {
                SirenixEditorGUI.ErrorMessageBox(error);
                CallNextDrawer(label);
            }
            else if (isList)
            {
                if (base.Attribute.DisableListAddButtonBehaviour)
                {
                    CallNextDrawer(label);
                    return;
                }

                Action nextCustomAddFunction = CollectionDrawerStaticInfo.NextCustomAddFunction;
                CollectionDrawerStaticInfo.NextCustomAddFunction = OpenSelector;
                CallNextDrawer(label);
                if (result != null)
                {
                    AddResult(result);
                    result = null;
                }

                CollectionDrawerStaticInfo.NextCustomAddFunction = nextCustomAddFunction;
            }
            else if (base.Attribute.DrawDropdownForListElements || !isListElement)
            {
                DrawDropdown();
            }
            else
            {
                CallNextDrawer(label);
            }
        }

        private void AddResult(IEnumerable<object> query)
        {
            if (isList)
            {
                var collectionResolver = base.Property.ChildResolver as ICollectionResolver;
                if (enableMultiSelect)
                {
                    collectionResolver?.QueueClear();
                }

                {
                    foreach (object item in query)
                    {
                        object[] array = new object[base.Property.ParentValues.Count];
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (base.Attribute.CopyValues)
                            {
                                array[i] = Sirenix.Serialization.SerializationUtility.CreateCopy(item);
                            }
                            else
                            {
                                array[i] = item;
                            }
                        }

                        collectionResolver?.QueueAdd(array);
                    }

                    return;
                }
            }

            object obj = query.FirstOrDefault();
            for (int j = 0; j < base.Property.ValueEntry.WeakValues.Count; j++)
            {
                if (base.Attribute.CopyValues)
                {
                    base.Property.ValueEntry.WeakValues[j] = Sirenix.Serialization.SerializationUtility.CreateCopy(obj);
                }
                else
                {
                    base.Property.ValueEntry.WeakValues[j] = obj;
                }
            }
        }

        private void DrawDropdown()
        {
            IEnumerable<object>? enumerable = null;
            if (base.Attribute.AppendNextDrawer && !isList)
            {
                GUILayout.BeginHorizontal();
                float num = 15f;
                if (label != null)
                {
                    num += GUIHelper.BetterLabelWidth;
                }

                GUIContent gUIContent = GUIHelper.TempContent("");
                if (base.Property.Info.TypeOfValue == typeof(Type))
                {
                    gUIContent.image = GUIHelper.GetAssetThumbnail(null, base.Property.ValueEntry.WeakSmartValue as Type, preferObjectPreviewOverFileIcon: false);
                }

                enumerable = OdinSelector<object>.DrawSelectorDropdown(label, gUIContent, ShowSelector, !base.Attribute.OnlyChangeValueOnConfirm, GUIStyle.none, GUILayoutOptions.Width(num));
                if (Event.current.type == EventType.Repaint)
                {
                    Rect position = GUILayoutUtility.GetLastRect().AlignRight(15f);
                    position.y += 4f;
                    SirenixGUIStyles.PaneOptions.Draw(position, GUIContent.none, 0);
                }

                GUILayout.BeginVertical();
                bool disableGUIInAppendedDrawer = base.Attribute.DisableGUIInAppendedDrawer;
                if (disableGUIInAppendedDrawer)
                {
                    GUIHelper.PushGUIEnabled(enabled: false);
                }

                CallNextDrawer(null);
                if (disableGUIInAppendedDrawer)
                {
                    GUIHelper.PopGUIEnabled();
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUIContent gUIContent2 = GUIHelper.TempContent(GetCurrentValueName());
                if (base.Property.Info.TypeOfValue == typeof(Type))
                {
                    gUIContent2.image = GUIHelper.GetAssetThumbnail(null, base.Property.ValueEntry.WeakSmartValue as Type, preferObjectPreviewOverFileIcon: false);
                }

                if (!base.Attribute.HideChildProperties && base.Property.Children.Count > 0)
                {
                    isToggled!.Value = SirenixEditorGUI.Foldout(isToggled.Value, label, out var valueRect);
                    enumerable = OdinSelector<object>.DrawSelectorDropdown(valueRect, gUIContent2, ShowSelector, !base.Attribute.OnlyChangeValueOnConfirm);
                    if (SirenixEditorGUI.BeginFadeGroup(this, isToggled.Value))
                    {
                        EditorGUI.indentLevel++;
                        for (int i = 0; i < base.Property.Children.Count; i++)
                        {
                            InspectorProperty inspectorProperty = base.Property.Children[i];
                            inspectorProperty.Draw(inspectorProperty.Label);
                        }

                        EditorGUI.indentLevel--;
                    }

                    SirenixEditorGUI.EndFadeGroup();
                }
                else
                {
                    enumerable = OdinSelector<object>.DrawSelectorDropdown(label, gUIContent2, ShowSelector, !base.Attribute.OnlyChangeValueOnConfirm, null);
                }
            }

            if (enumerable != null && enumerable.Any())
            {
                AddResult(enumerable);
            }
        }

        private void OpenSelector()
        {
            ReloadDropdownCollections();
            Rect rect = new Rect(Event.current.mousePosition, Vector2.zero);
            OdinSelector<object> odinSelector = ShowSelector(rect);
            odinSelector.SelectionConfirmed += delegate (IEnumerable<object> x)
            {
                result = x;
            };
        }

        private OdinSelector<object> ShowSelector(Rect rect)
        {
            GenericSelector<object> genericSelector = CreateSelector();
            rect.x = (int)rect.x;
            rect.y = (int)rect.y;
            rect.width = (int)rect.width;
            rect.height = (int)rect.height;
            if (base.Attribute.AppendNextDrawer && !isList)
            {
                rect.xMax = GUIHelper.GetCurrentLayoutRect().xMax;
            }

            genericSelector.ShowInPopup(rect, new Vector2(base.Attribute.DropdownWidth, base.Attribute.DropdownHeight));
            return genericSelector;
        }

        private GenericSelector<object> CreateSelector()
        {
            bool isUniqueList = base.Attribute.IsUniqueList;
            IEnumerable<ValueDropdownItem> enumerable = getValues?.Invoke() ?? Enumerable.Empty<ValueDropdownItem>();
            if (enumerable.Any())
            {
                if ((isList && base.Attribute.ExcludeExistingValuesInList) || (isListElement && isUniqueList))
                {
                    List<ValueDropdownItem> list = enumerable.ToList();
                    InspectorProperty inspectorProperty = base.Property.FindParent((InspectorProperty x) => x.ChildResolver is ICollectionResolver, includeSelf: true);
                    IValueDropdownExEqualityComparer comparer = new IValueDropdownExEqualityComparer(isTypeLookup: false);
                    inspectorProperty.ValueEntry.WeakValues.Cast<IEnumerable>().SelectMany((IEnumerable x) => x.Cast<object>()).ForEach(delegate (object x)
                    {
                        list.RemoveAll((ValueDropdownItem c) => comparer.Equals(c, x));
                    });
                    enumerable = list;
                }

                if (nameLookup != null)
                {
                    foreach (ValueDropdownItem item in enumerable)
                    {
                        if (item.Value != null)
                        {
                            nameLookup[item.Value] = item.Text;
                        }
                    }
                }
            }

            bool drawSearchToolbar = base.Attribute.NumberOfItemsBeforeEnablingSearch == 0 || (enumerable != null && enumerable.Take(base.Attribute.NumberOfItemsBeforeEnablingSearch).Count() == base.Attribute.NumberOfItemsBeforeEnablingSearch);
            GenericSelector<object> genericSelector = new GenericSelector<object>(base.Attribute.DropdownTitle, supportsMultiSelect: false, enumerable.Select((ValueDropdownItem x) => new GenericSelectorItem<object>(x.Text, x.Value)));
            enableMultiSelect = isList && isUniqueList && !base.Attribute.ExcludeExistingValuesInList;
            if (base.Attribute.FlattenTreeView)
            {
                genericSelector.FlattenedTree = true;
            }

            if (isList && !base.Attribute.ExcludeExistingValuesInList && isUniqueList)
            {
                genericSelector.CheckboxToggle = true;
            }
            else if (!base.Attribute.DoubleClickToConfirm && !enableMultiSelect)
            {
                genericSelector.EnableSingleClickToSelect();
            }

            if (isList && enableMultiSelect)
            {
                genericSelector.SelectionTree.Selection.SupportsMultiSelect = true;
                genericSelector.DrawConfirmSelectionButton = true;
            }

            genericSelector.SelectionTree.Config.DrawSearchToolbar = drawSearchToolbar;
            IEnumerable<object> selection = Enumerable.Empty<object>();
            if (!isList)
            {
                selection = getSelection?.Invoke() ?? selection;
            }
            else if (enableMultiSelect)
            {
                selection = getSelection?.Invoke().SelectMany((object x) => (x as IEnumerable).Cast<object>()) ?? selection;
            }

            genericSelector.SetSelection(selection);
            if (base.Attribute.ItemAction is null)
            {
                genericSelector.SelectionTree.EnumerateTree().AddThumbnailIcons(preferAssetPreviewAsIcon: true);
            }
            else
            {
                genericSelector.SelectionTree.EnumerateTree(x =>
                {
                    itemAction?.Context.NamedValues.Set("item", x);
                    itemAction?.DoAction();

                    if (x.Icon == null)
                    {
                        x.AddThumbnailIcon(preferAssetPreviewAsIcon: true);
                    }
                });
            }
            
            if (base.Attribute.ExpandAllMenuItems)
            {
                genericSelector.SelectionTree.EnumerateTree(delegate (OdinMenuItem x)
                {
                    x.Toggled = true;
                });
            }

            if (base.Attribute.SortDropdownItems)
            {
                genericSelector.SelectionTree.SortMenuItemsByName();
            }

            return genericSelector;
        }

        private string GetCurrentValueName()
        {
            if (!EditorGUI.showMixedValue)
            {
                object weakSmartValue = base.Property.ValueEntry.WeakSmartValue;
                string? value = null;
                if (nameLookup != null && weakSmartValue != null)
                {
                    nameLookup.TryGetValue(weakSmartValue, out value);
                }

                return new GenericSelectorItem<object>(value, weakSmartValue!).GetNiceName();
            }

            return "—";
        }
    }
}

#endif