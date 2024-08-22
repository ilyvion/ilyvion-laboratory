// Copyright (c) 2022 bradson
// Copyright (c) 2024 Alexander Krivács Schrøder
// Taken from
// <https://github.com/bbradson/Xenotype-Spawn-Control/blob/main/Source/ModSettingsWindow.cs>
// and adapted to my needs

using HarmonyLib;

namespace ilyvion.Laboratory.UI;

public abstract class Tab
{
    public virtual string Title => GetType().Name;
    public virtual bool Show => true;
    public abstract void DoTabContents(Rect inRect);
}

public class TabRecord : Verse.TabRecord
{
    public Tab Tab { get; }
    public AccessTools.FieldRef<Tab> CurrentTab { get; }
    public TabRecord(Tab tab, AccessTools.FieldRef<Tab> currentTab)
        : base(ValidateTabTitle(tab), null, null)
    {
        Tab = tab;
        CurrentTab = currentTab;
        clickedAction = () => CurrentTab() = Tab;
        selectedGetter = () => CurrentTab() == Tab;
    }

    private static string ValidateTabTitle(Tab tab)
    {
        if (tab == null)
        {
            throw new ArgumentNullException(nameof(tab));
        }
        return tab.Title;
    }
}
