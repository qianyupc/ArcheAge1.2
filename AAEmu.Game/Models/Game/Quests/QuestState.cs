﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AAEmu.Game.Core.Managers;
using AAEmu.Game.Core.Managers.World;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.Models.Game.Quests.Acts;
using AAEmu.Game.Models.Game.Quests.Static;
using AAEmu.Game.Models.Game.Quests.Templates;
using AAEmu.Game.Models.Game.Units;
using AAEmu.Game.Models.Game.World;
using AAEmu.Game.Utils;
using NLog;

namespace AAEmu.Game.Models.Game.Quests;

// класс, определяющий состояние
// class defining the state
public abstract class QuestState
{
    protected static Logger Logger = LogManager.GetCurrentClassLogger();

    public Quest Quest { get; set; }
    public CurrentQuestComponent CurrentQuestComponent { get; set; }
    public List<QuestComponent> CurrentComponents { get; set; } = new();
    public List<QuestAct> CurrentActs { get; set; } = new();
    public QuestComponent CurrentComponent { get; set; }
    public QuestComponentKind Step { get; set; }
    //public List<bool> ContextResults { get; set; }
    public abstract bool Start(bool forcibly = false);
    public abstract bool Update();
    public abstract bool Complete(int selected = 0, EventArgs eventArgs = null);
    public abstract void Fail();
    public abstract void Drop();

    public void UpdateContext(Quest quest, QuestState questState, QuestContext questContext, QuestComponentKind questComponentKind)
    {
        var exit = false;

        // необходимо проверить, какие шаги имеются
        // need to check what steps are available
        for (var step = questComponentKind; step <= QuestComponentKind.Reward; step++)
        {
            var questComponents = quest.Template.GetComponents(step);
            if (questComponents.Length == 0) { break; }
            switch (step)
            {
                case QuestComponentKind.None:
                    {
                        quest.QuestNoneState = questContext;
                        quest.QuestNoneState.State = this;
                        quest.QuestNoneState.State.Quest = quest;
                        quest.QuestNoneState.State.Step = step;
                        quest.QuestNoneState.State.CurrentQuestComponent = UpdateComponent(questComponents);
                        quest.QuestNoneState.State.CurrentComponents = UpdateComponents();
                        quest.QuestNoneState.State.CurrentActs = UpdateActs();
                        //quest.QuestNoneState.State.ContextResults = new List<bool>();
                        //for (var i = 0; i < quest.QuestNoneState.State.CurrentComponents.Count; i++)
                        //{
                        //    quest.QuestNoneState.State.ContextResults.Add(false);
                        //}
                        exit = true;
                        break;
                    }
                case QuestComponentKind.Start:
                    {
                        quest.QuestStartState = questContext;
                        quest.QuestStartState.State = this;
                        quest.QuestStartState.State.Quest = quest;
                        quest.QuestStartState.State.Step = step;
                        quest.QuestStartState.State.CurrentQuestComponent = UpdateComponent(questComponents);
                        quest.QuestStartState.State.CurrentComponents = UpdateComponents();
                        quest.QuestStartState.State.CurrentActs = UpdateActs();
                        //quest.QuestStartState.State.ContextResults = new List<bool>();
                        //for (var i = 0; i < quest.QuestStartState.State.CurrentComponents.Count; i++)
                        //{
                        //    quest.QuestStartState.State.ContextResults.Add(false);
                        //}
                        exit = true;
                        break;
                    }
                case QuestComponentKind.Supply:
                    {
                        quest.QuestSupplyState = questContext;
                        quest.QuestSupplyState.State = this;
                        quest.QuestSupplyState.State.Quest = quest;
                        quest.QuestSupplyState.State.Step = step;
                        quest.QuestSupplyState.State.CurrentQuestComponent = UpdateComponent(questComponents);
                        quest.QuestSupplyState.State.CurrentComponents = UpdateComponents();
                        quest.QuestSupplyState.State.CurrentActs = UpdateActs();
                        //quest.QuestSupplyState.State.ContextResults = new List<bool>();
                        //for (var i = 0; i < quest.QuestSupplyState.State.CurrentComponents.Count; i++)
                        //{
                        //    quest.QuestSupplyState.State.ContextResults.Add(false);
                        //}
                        exit = true;
                        break;
                    }
                case QuestComponentKind.Progress:
                    {
                        quest.QuestProgressState = questContext;
                        quest.QuestProgressState.State = this;
                        quest.QuestProgressState.State.Quest = quest;
                        quest.QuestProgressState.State.Step = step;
                        quest.QuestProgressState.State.CurrentQuestComponent = UpdateComponent(questComponents);
                        quest.QuestProgressState.State.CurrentComponents = UpdateComponents();
                        quest.QuestProgressState.State.CurrentActs = UpdateActs();
                        //quest.QuestProgressState.State.ContextResults = new List<bool>();
                        //for (var i = 0; i < quest.QuestProgressState.State.CurrentComponents.Count; i++)
                        //{
                        //    quest.QuestProgressState.State.ContextResults.Add(false);
                        //}
                        exit = true;
                        break;
                    }
                case QuestComponentKind.Fail:
                    break;
                case QuestComponentKind.Ready:
                    {
                        quest.QuestReadyState = questContext;
                        quest.QuestReadyState.State = this;
                        quest.QuestReadyState.State.Quest = quest;
                        quest.QuestReadyState.State.Step = step;
                        quest.QuestReadyState.State.CurrentQuestComponent = UpdateComponent(questComponents);
                        quest.QuestReadyState.State.CurrentComponents = UpdateComponents();
                        quest.QuestReadyState.State.CurrentActs = UpdateActs();
                        //quest.QuestReadyState.State.ContextResults = new List<bool>();
                        //for (var i = 0; i < quest.QuestReadyState.State.CurrentComponents.Count; i++)
                        //{
                        //    quest.QuestReadyState.State.ContextResults.Add(false);
                        //}
                        exit = true;
                        break;
                    }
                case QuestComponentKind.Drop:
                    break;
                case QuestComponentKind.Reward:
                    {
                        quest.QuestRewardState = questContext;
                        quest.QuestRewardState.State = this;
                        quest.QuestRewardState.State.Quest = quest;
                        quest.QuestRewardState.State.Step = step;
                        quest.QuestRewardState.State.CurrentQuestComponent = UpdateComponent(questComponents);
                        quest.QuestRewardState.State.CurrentComponents = UpdateComponents();
                        quest.QuestRewardState.State.CurrentActs = UpdateActs();
                        //quest.QuestRewardState.State.ContextResults = new List<bool>();
                        //for (var i = 0; i < quest.QuestRewardState.State.CurrentComponents.Count; i++)
                        //{
                        //    quest.QuestRewardState.State.ContextResults.Add(false);
                        //}
                        exit = true;
                        break;
                    }
            }
            if (exit) { break; }
        }

        return;

        CurrentQuestComponent UpdateComponent(QuestComponent[] components)
        {
            // собираем компоненты для шага квеста
            // collect components for the quest step
            CurrentQuestComponent = new CurrentQuestComponent();
            foreach (var component in components)
            {
                CurrentQuestComponent.Add(component);
            }

            return CurrentQuestComponent;
        }
        List<QuestComponent> UpdateComponents()
        {
            CurrentComponents = CurrentQuestComponent.GetComponents();
            return CurrentComponents;
        }
        List<QuestAct> UpdateActs()
        {
            foreach (var component in CurrentComponents)
            {
                var acts = QuestManager.Instance.GetActs(component.Id);
                foreach (var act in acts)
                {
                    CurrentActs.Add((QuestAct)act);
                }
            }
            return CurrentActs;
        }
    }
}

// Конкретные классы, представляющие различные состояния
// Concrete classes representing different states
public class QuestNoneState : QuestState
{
    public override bool Start(bool forcibly = false)
    {
        Logger.Info($"[QuestNoneState][Start] Quest: {Quest.TemplateId} начался!");

        var results = new List<bool>();
        if (forcibly)
        {
            results.Add(true);
        }
        else
        {
            results = CurrentQuestComponent.Execute(Quest.Owner, Quest, 0);
        }

        CurrentComponent = CurrentQuestComponent.GetFirstComponent();
        if (results.Any(b => b == true))
        {
            Quest.ComponentId = CurrentComponent.Id;

            if (Quest.QuestProgressState.State.CurrentQuestComponent != null)
            {
                // если есть шаг Progress, то не надо завершать квест
                Quest.Status = QuestStatus.Progress;
                //Quest.Condition = QuestConditionObj.Progress;
            }
            else
            {
                Quest.Status = results.Any(b => b == true) ? QuestStatus.Ready : QuestStatus.Progress;
            }
            switch (Quest.Status)
            {
                case QuestStatus.Progress:
                case QuestStatus.Ready:
                    //Quest.Condition = QuestConditionObj.Progress;
                    //Quest.Step++; // Supply
                    break;
                default:
                    Quest.Step = QuestComponentKind.Fail;
                    //Quest.Condition = QuestConditionObj.Fail;
                    break;
            }

            Logger.Info($"[QuestNoneState][Start] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
        }
        else
        {
            Logger.Info($"[QuestNoneState][Start] Quest: {Quest.TemplateId} start failed.");
            Logger.Info($"[QuestNoneState][Start] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
            return false; // останавливаемся на этом шаге, сигнал на удаление квеста
        }
        Quest.UseSkillAndBuff(CurrentComponent);

        return true;
    }
    public override bool Update()
    {
        Logger.Info($"[QuestNoneState][Update] Quest: {Quest.TemplateId}. Ничего не делаем.");
        Logger.Info($"[QuestNoneState][Update] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
        return true;
    }
    public override bool Complete(int selected = 0, EventArgs eventArgs = null)
    {
        Logger.Info($"[QuestNoneState][Complete] Quest: {Quest.TemplateId}. Шаг успешно завершен!");
        Logger.Info($"[QuestNoneState][Complete] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
        return true;
    }
    public override void Fail()
    {
        Logger.Info($"[QuestNoneState][Fail] Quest: {Quest.TemplateId} не может завершиться неудачей, пока не начался!");
        Logger.Info($"[QuestNoneState][Fail] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
    }
    public override void Drop()
    {
        Logger.Info($"[QuestNoneState][Drop] Квест {Quest.TemplateId} сброшен");
        Logger.Info($"[QuestNoneState][Drop] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
    }
}
public class QuestStartState : QuestState
{
    public override bool Start(bool forcibly = false)
    {
        Logger.Info($"[QuestStartState][Start] Quest: {Quest.TemplateId} начался!");

        var results = new List<bool>();
        if (forcibly)
        {
            results.Add(true); // применяем квест насильно командой '/quest add <questId>', даже если нет рядом нужного Npc
        }
        else
        {
            results = CurrentQuestComponent.Execute(Quest.Owner, Quest, 0);
        }

        foreach (var component in CurrentComponents)
        {
            if (results.Any(b => b == true))
            {
                Quest.ComponentId = component.Id;

                if (Quest.QuestProgressState.State.CurrentQuestComponent != null)
                {
                    // если есть шаг Progress, то не надо завершать квест
                    Quest.Status = QuestStatus.Progress;

                    // проверим, что есть на этом шаге акт QuestActObjSphere
                    var progressContexts = Quest.QuestProgressState.State.CurrentQuestComponent.GetComponents();
                    foreach (var progressContext in progressContexts)
                    {
                        var actss = QuestManager.Instance.GetActs(progressContext.Id);
                        foreach (var act in actss)
                        {
                            switch (act?.DetailType)
                            {
                                case "QuestActObjSphere":
                                    {
                                        // подготовим работу QuestSphere
                                        // prepare QuestSphere's work
                                        Logger.Info($"[QuestStartState][Start] Quest: {Quest.TemplateId}. Подписываемся на события, которые требуются для работы сферы");
                                        Quest.CurrentComponentId = progressContext.Id;
                                        var spheres = SphereQuestManager.Instance.GetQuestSpheres(progressContext.Id);
                                        if (spheres != null)
                                        {
                                            foreach (var sphere in spheres)
                                            {
                                                var sphereQuestTrigger = new SphereQuestTrigger();
                                                sphereQuestTrigger.Sphere = sphere;

                                                if (sphereQuestTrigger.Sphere == null)
                                                {
                                                    Logger.Info($"[QuestStartState][Start] QuestActObjSphere: Sphere not found with cquest {CurrentComponent.Id} in quest_sign_spheres.json!");
                                                    break;
                                                }

                                                sphereQuestTrigger.Owner = Quest.Owner;
                                                sphereQuestTrigger.Quest = Quest;
                                                sphereQuestTrigger.TickRate = 500;

                                                SphereQuestManager.Instance.AddSphereQuestTrigger(sphereQuestTrigger);
                                            }

                                            const int Duration = 500;
                                            // TODO : Add a proper delay in here
                                            Task.Run(async () =>
                                            {
                                                await Task.Delay(Duration);
                                            });

                                            // подписка одна на всех
                                            Quest.Owner.Events.OnEnterSphere -= Quest.Owner.Quests.OnEnterSphereHandler;
                                            Quest.Owner.Events.OnEnterSphere += Quest.Owner.Quests.OnEnterSphereHandler;

                                            Logger.Info($"[QuestStartState][Start] Quest: {Quest.TemplateId}, Event: 'OnEnterSphere', Handler: 'OnEnterSphereHandler'");
                                            break;
                                        }

                                        // если сфера по какой-то причине отсутствует, будем считать, что мы её посетили
                                        // if the sphere is missing for some reason, we will assume that we have visited it
                                        Quest.Owner.SendMessage("[Quest] Quest {Quest.TemplateId}, Sphere not found - skipped..");
                                        Logger.Info($"[QuestStartState][Start] Quest {Quest.TemplateId}, Sphere not found - skipped..");
                                        break;
                                    }
                            }
                        }
                    }
                }
                else
                {
                    Quest.Status = results.Any(b => b == true) ? QuestStatus.Ready : QuestStatus.Progress;
                }

                switch (Quest.Status)
                {
                    case QuestStatus.Progress:
                    case QuestStatus.Ready:
                        //Quest.Condition = QuestConditionObj.Progress;
                        break;
                    default:
                        Quest.Step = QuestComponentKind.Fail;
                        //Quest.Condition = QuestConditionObj.Fail;
                        break;
                }

                Logger.Info($"[QuestStartState][Start] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
            }
            else
            {
                Logger.Info($"[QuestStartState][Start] Quest: {Quest.TemplateId} start failed.");
                Logger.Info($"[QuestStartState][Start] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
                return false; // останавливаемся на этом шаге, сигнал на удаление квеста
            }

            Quest.UseSkillAndBuff(component);
        }

        return true;
    }
    public override bool Update()
    {
        Logger.Info($"[QuestStartState][Update] Quest: {Quest.TemplateId}. Ничего не делаем.");
        Logger.Info($"[QuestStartState][Update] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");

        return true;
    }
    public override bool Complete(int selected = 0, EventArgs eventArgs = null)
    {
        //Quest.Step = QuestComponentKind.Start;
        Logger.Info($"[QuestStartState][Complete] Quest: {Quest.TemplateId}. Шаг успешно завершен!");
        Logger.Info($"[QuestStartState][Complete] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");

        return true;
    }
    public override void Fail()
    {
        Logger.Info($"[QuestStartState][Fail] Quest: {Quest.TemplateId} не может завершиться неудачей, пока не начался!");
        Logger.Info($"[QuestStartState][Fail] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
    }
    public override void Drop()
    {
        Logger.Info($"[QuestStartState][Drop] Quest: {Quest.TemplateId} сброшен");
        Logger.Info($"[QuestStartState][Drop] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
    }
}
public class QuestSupplyState : QuestState
{
    public override bool Start(bool forcibly = false)
    {
        Logger.Info($"[QuestSupplyState][Start] Quest: {Quest.TemplateId} в процессе выполнения.");
        Logger.Info($"[QuestSupplyState][Start] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
        return true;
    }
    public override bool Update()
    {
        Logger.Info($"[QuestSupplyState][Update] Quest: {Quest.TemplateId}. Получим нужные предметы для прохождения квеста.");
        CurrentQuestComponent.Execute(Quest.Owner, Quest, 0); // получим квестовые предметы // get quest items

        //Logger.Info($"[QuestSupplyState][Update] Quest: {Quest.TemplateId}. Подписываемся на события.");
        //// подписка одна на всех
        //Quest.Owner.Events.OnItemGather -= Quest.Owner.Quests.OnItemGatherHandler;
        //Quest.Owner.Events.OnItemGather += Quest.Owner.Quests.OnItemGatherHandler;
        //Logger.Info($"[QuestSupplyState][Update] Quest: {Quest.TemplateId}, Event: 'OnItemGather', Handler: 'OnItemGatherHandler'");

        //foreach (var component in CurrentComponents)
        //{
        //    var acts = QuestManager.Instance.GetActs(component.Id);

        //    foreach (var act in acts)
        //    {
        //        switch (act?.DetailType)
        //        {
        //            case "QuestActSupplyItem":
        //                {
        //                    var template = act.GetTemplate<QuestActSupplyItem>(); // для доступа к переменным требуется привидение к нужному типу
        //                    // инициируем событие
        //                    Quest.Owner?.Events?.OnItemGather(this, new OnItemGatherArgs
        //                    {
        //                        QuestId = Quest.TemplateId,
        //                        ItemId = template.ItemId,
        //                        Count = template.Count
        //                    });
        //                    break;
        //                }
        //        }
        //    }
        //}

        ////Quest.Status = QuestStatus.Ready;
        ////Quest.Condition = QuestConditionObj.Ready;
        //Quest.Step = QuestComponentKind.Supply;
        Logger.Info($"[QuestSupplyState][Update] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");

        return true;
    }
    public override bool Complete(int selected = 0, EventArgs eventArgs = null)
    {
        Logger.Info($"[QuestSupplyState][Complete] Quest: {Quest.TemplateId}. Шаг успешно завершен!");
        //Quest.Step = QuestComponentKind.Supply;
        Logger.Info($"[QuestSupplyState][Complete] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");

        return true;
    }
    public override void Fail()
    {
        Logger.Info($"[QuestSupplyState][Fail] Quest: {Quest.TemplateId} провален");
        Logger.Info($"[QuestSupplyState][Fail] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
    }
    public override void Drop()
    {
        Logger.Info($"[QuestSupplyState][Drop] Quest: {Quest.TemplateId} сброшен");
        Logger.Info($"[QuestSupplyState][Drop] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
    }
}
public class QuestProgressState : QuestState
{
    public override bool Start(bool forcibly = false)
    {
        Logger.Info($"[QuestProgressState][Start] Quest: {Quest.TemplateId} уже в процессе выполнения.");
        Logger.Info($"[QuestProgressState][Start] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
        return true;
    }

    //public int GetObjectiveCount(QuestAct act, clatt actDetailType)
    //{
    //    // нужно посмотреть в инвентарь, так как после Start() ещё не знаем, есть предмет в инвентаре или нет (we need to look in the inventory, because after Start() we don't know yet if the item is in the inventory or not)
    //    var objectiveCount = Quest.Owner.Inventory.GetItemsCount(template.ItemId);

    //    return 0;
    //}

    public override bool Update()
    {
        //// нужно посмотреть в инвентарь, так как ещё не знаем, есть предмет в инвентаре или нет
        //// we need to look in the inventory, because we don't know yet if the item is in the inventory or not
        //var objectiveCount = Quest.GetObjectives(Step);

        ////var result = CurrentActs.Select(act => act.Use(Quest.Owner, Quest, 0)).ToList();
        //var results = CurrentQuestComponent.Execute(Quest.Owner, Quest, objectiveCount[0]);

        CurrentComponent = CurrentQuestComponent.GetFirstComponent();

        //if (results.All(b => b == true))
        //{
        //    Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}. Не надо подписываться на события.");
        //    Quest.ComponentId = CurrentComponent.Id;
        //    Quest.Status = QuestStatus.Ready;
        //    Quest.Condition = QuestConditionObj.Ready;
        //    Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");

        //    Quest.Owner.SendPacket(new SCQuestContextUpdatedPacket(Quest, Quest.ComponentId));

        //    return true;
        //}

        Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}. Подписываемся на события, которые требуются для активных актов");
        bool res;
        var results2 = new List<bool>();

        foreach (var component in CurrentComponents)
        {
            var acts = QuestManager.Instance.GetActs(component.Id);

            foreach (var act in acts)
            {
                switch (act?.DetailType)
                {
                    case "QuestActObjMonsterHunt":
                        {
                            //// нужно посмотреть в инвентарь, так как ещё не знаем, есть предмет в инвентаре или нет
                            //// we need to look in the inventory, because we don't know yet if the item is in the inventory or not
                            //res = CheckCount(act);
                            ////result2 = act.Template.IsCompleted();
                            //if (res)
                            //{
                            //    results2.Add(true); // уже выполнили задание, выход
                            //    break;
                            //}

                            // подписка одна на всех
                            Quest.Owner.Events.OnMonsterHunt -= Quest.Owner.Quests.OnMonsterHuntHandler;
                            Quest.Owner.Events.OnMonsterHunt += Quest.Owner.Quests.OnMonsterHuntHandler;

                            Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Event: 'OnMonsterHunt', Handler: 'OnMonsterHuntHandler'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                    case "QuestActObjMonsterGroupHunt":
                        {
                            //// нужно посмотреть в инвентарь, так как ещё не знаем, есть предмет в инвентаре или нет
                            //// we need to look in the inventory, because we don't know yet if the item is in the inventory or not
                            //res = CheckCount(act);
                            ////result2 = act.Template.IsCompleted();
                            //if (res)
                            //{
                            //    results2.Add(true); // уже выполнили задание, выход
                            //    break;
                            //}

                            // подписка одна на всех
                            Quest.Owner.Events.OnMonsterGroupHunt -= Quest.Owner.Quests.OnMonsterGroupHuntHandler;
                            Quest.Owner.Events.OnMonsterGroupHunt += Quest.Owner.Quests.OnMonsterGroupHuntHandler;

                            Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Event: 'OnMonsterGroupHunt', Handler: 'OnMonsterGroupHuntHandler'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                    case "QuestActObjItemGather":
                        {
                            // нужно посмотреть в инвентарь, так как ещё не знаем, есть предмет в инвентаре или нет
                            // we need to look in the inventory, because we don't know yet if the item is in the inventory or not
                            res = CheckCount(act);
                            //result2 = act.Template.IsCompleted();
                            if (res)
                            {
                                results2.Add(true); // уже выполнили задание, выход
                                break;
                            }

                            // подписка одна на всех
                            Quest.Owner.Events.OnItemGather -= Quest.Owner.Quests.OnItemGatherHandler;
                            Quest.Owner.Events.OnItemGather += Quest.Owner.Quests.OnItemGatherHandler;

                            Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Event: 'OnItemGather', Handler: 'OnItemGatherHandler'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                    case "QuestActObjItemGroupGather":
                        {
                            // нужно посмотреть в инвентарь, так как ещё не знаем, есть предмет в инвентаре или нет
                            // we need to look in the inventory, because we don't know yet if the item is in the inventory or not
                            res = CheckCount(act);
                            //result2 = act.Template.IsCompleted();
                            if (res)
                            {
                                Logger.Info($"[QuestProgressState][Update][QuestActObjItemGroupGather] Quest: {Quest.TemplateId}. Подписываться на событие не надо, так как в инвентаре уже лежать нужные вещи.");
                                results2.Add(true); // уже выполнили задание, выход
                                break;
                            }

                            // подписка одна на всех
                            Quest.Owner.Events.OnItemGroupGather -= Quest.Owner.Quests.OnItemGroupGatherHandler;
                            Quest.Owner.Events.OnItemGroupGather += Quest.Owner.Quests.OnItemGroupGatherHandler;

                            Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Event: 'OnItemGroupGather', Handler: 'OnItemGroupGatherHandler'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                    case "QuestActObjItemUse":
                        {
                            //// нужно посмотреть в инвентарь, так как ещё не знаем, есть предмет в инвентаре или нет
                            //// we need to look in the inventory, because we don't know yet if the item is in the inventory or not
                            //res = CheckCount(act);
                            ////result2 = act.Template.IsCompleted();
                            //if (res)
                            //{
                            //    results2.Add(true); // уже выполнили задание, выход
                            //    break;
                            //}

                            // подписка одна на всех
                            Quest.Owner.Events.OnItemUse -= Quest.Owner.Quests.OnItemUseHandler;
                            Quest.Owner.Events.OnItemUse += Quest.Owner.Quests.OnItemUseHandler;

                            Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Event: 'OnItemUse', Handler: 'OnItemUseHandler'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                    case "QuestActObjItemGroupUse":
                        {
                            //// нужно посмотреть в инвентарь, так как ещё не знаем, есть предмет в инвентаре или нет
                            //// we need to look in the inventory, because we don't know yet if the item is in the inventory or not
                            //res = CheckCount(act);
                            ////result2 = act.Template.IsCompleted();
                            //if (res)
                            //{
                            //    results2.Add(true); // уже выполнили задание, выход
                            //    break;
                            //}

                            // подписка одна на всех
                            Quest.Owner.Events.OnItemGroupUse -= Quest.Owner.Quests.OnItemGroupUseHandler;
                            Quest.Owner.Events.OnItemGroupUse += Quest.Owner.Quests.OnItemGroupUseHandler;

                            Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Event: 'OnItemGroupUse', Handler: 'OnItemGroupUseHandler'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                    case "QuestActObjInteraction":
                        {
                            //// нужно посмотреть в инвентарь, так как ещё не знаем, есть предмет в инвентаре или нет
                            //// we need to look in the inventory, because we don't know yet if the item is in the inventory or not
                            //res = CheckCount(act);
                            ////result2 = act.Template.IsCompleted();
                            //if (res)
                            //{
                            //    results2.Add(true); // уже выполнили задание, выход
                            //    break;
                            //}

                            // подписка одна на всех
                            Quest.Owner.Events.OnInteraction -= Quest.Owner.Quests.OnInteractionHandler;
                            Quest.Owner.Events.OnInteraction += Quest.Owner.Quests.OnInteractionHandler;

                            Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Event: 'OnInteraction', Handler: 'OnInteractionHandler'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                    case "QuestActObjLevel":
                        {
                            //// нужно посмотреть в инвентарь, так как ещё не знаем, есть предмет в инвентаре или нет
                            //// we need to look in the inventory, because we don't know yet if the item is in the inventory or not
                            //res = CheckCount(act);
                            ////result2 = act.Template.IsCompleted();
                            //if (res)
                            //{
                            //    results2.Add(true); // уже выполнили задание, выход
                            //    break;
                            //}

                            // подписка одна на всех
                            Quest.Owner.Events.OnLevelUp -= Quest.Owner.Quests.OnLevelUpHandler;
                            Quest.Owner.Events.OnLevelUp += Quest.Owner.Quests.OnLevelUpHandler;

                            Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Event: 'OnLevelUp', Handler: 'OnLevelUpHandler'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                    case "QuestActObjMateLevel":
                        {
                            //Quest.Owner.Events.OnMateLevel += Quest.Owner.Quests.OnMateLevelHandler;
                            //result2 = false;
                            break;
                        }
                    case "QuestActObjEffectFire":
                        {
                            //Quest.Owner.Events.OnMateLevel += Quest.Owner.Quests.OnMateLevelHandler;
                            //result2 = false;
                            break;
                        }
                    case "QuestActObjSendMail":
                        {
                            //Quest.Owner.Events.OnMateLevel += Quest.Owner.Quests.OnMateLevelHandler;
                            //result2 = false;
                            break;
                        }
                    case "QuestActObjZoneKill":
                        {
                            //Quest.Owner.Events.OnMateLevel += Quest.Owner.Quests.OnMateLevelHandler;
                            //result2 = false;
                            break;
                        }
                    case "QuestActObjZoneMonsterHunt":
                        {
                            //Quest.Owner.Events.OnMateLevel += Quest.Owner.Quests.OnMateLevelHandler;
                            //result2 = false;
                            break;
                        }
                    case "QuestActObjZoneNpcTalk":
                        {
                            //Quest.Owner.Events.OnMateLevel += Quest.Owner.Quests.OnMateLevelHandler;
                            //result2 = false;
                            break;
                        }
                    case "QuestActObjZoneQuestComplete":
                        {
                            //Quest.Owner.Events.OnMateLevel += Quest.Owner.Quests.OnMateLevelHandler;
                            //result2 = false;
                            break;
                        }
                    case "QuestActObjSphere":
                        {
                            // На шаге Start уже подписальсь на событие

                            //// подготовим работу QuestSphere
                            //// prepare QuestSphere's work
                            ////Quest.Status = QuestStatus.Progress;
                            Quest.CurrentComponentId = component.Id;
                            //var spheres = SphereQuestManager.Instance.GetQuestSpheres(CurrentComponent.Id);
                            //if (spheres != null)
                            //{
                            //    foreach (var sphere in spheres)
                            //    {
                            //        var sphereQuestTrigger = new SphereQuestTrigger();
                            //        sphereQuestTrigger.Sphere = sphere;

                            //        if (sphereQuestTrigger.Sphere == null)
                            //        {
                            //            Logger.Info($"[Quest] QuestActObjSphere: Sphere not found with cquest {CurrentComponent.Id} in quest_sign_spheres.json!");
                            //            results2.Add(true); // уже выполнили задание, выход
                            //            break;
                            //        }

                            //        sphereQuestTrigger.Owner = Quest.Owner;
                            //        sphereQuestTrigger.Quest = Quest;
                            //        sphereQuestTrigger.TickRate = 500;

                            //        SphereQuestManager.Instance.AddSphereQuestTrigger(sphereQuestTrigger);
                            //    }

                            //    const int Duration = 500;
                            //    // TODO : Add a proper delay in here
                            //    Task.Run(async () =>
                            //    {
                            //        await Task.Delay(Duration);
                            //    });

                            // подписка одна на всех
                            Quest.Owner.Events.OnEnterSphere -= Quest.Owner.Quests.OnEnterSphereHandler;
                            Quest.Owner.Events.OnEnterSphere += Quest.Owner.Quests.OnEnterSphereHandler;

                            Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Event: 'OnEnterSphere', Handler: 'OnEnterSphereHandler'");
                            results2.Add(false); // будем ждать события
                            //    break;
                            //}

                            //// если сфера по какой-то причине отсутствует, будем считать, что мы её посетили
                            //// if the sphere is missing for some reason, we will assume that we have visited it
                            ////Quest.Status = QuestStatus.Progress;
                            //Quest.Owner.SendMessage($"[Quest] Quest {Quest.TemplateId}, Sphere not found - skipped..");
                            //Logger.Info($"[QuestProgressState][Update] Quest {Quest.TemplateId}, Sphere not found - skipped..");
                            //results2.Add(true); // не будем ждать события
                            break;
                        }
                    case "QuestActObjTalk":
                        {
                            // нужно посмотреть в инвентарь, так как ещё не знаем, есть предмет в инвентаре или нет
                            // we need to look in the inventory, because we don't know yet if the item is in the inventory or not
                            res = CheckCount(act);
                            //result2 = act.Template.IsCompleted();
                            if (res)
                            {
                                results2.Add(true); // уже выполнили задание, выход
                                break;
                            }

                            // подписка одна на всех
                            Quest.Owner.Events.OnTalkMade -= Quest.Owner.Quests.OnTalkMadeHandler;
                            Quest.Owner.Events.OnTalkMade += Quest.Owner.Quests.OnTalkMadeHandler;

                            Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Event: 'OnTalkMade', Handler: 'OnTalkMadeHandler'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                    case "QuestActObjTalkNpcGroup":
                        {
                            // нужно посмотреть в инвентарь, так как ещё не знаем, есть предмет в инвентаре или нет
                            // we need to look in the inventory, because we don't know yet if the item is in the inventory or not
                            res = CheckCount(act);
                            //result2 = act.Template.IsCompleted();
                            if (res)
                            {
                                results2.Add(true); // уже выполнили задание, выход
                                break;
                            }

                            // подписка одна на всех
                            Quest.Owner.Events.OnTalkNpcGroupMade -= Quest.Owner.Quests.OnTalkNpcGroupMadeHandler;
                            Quest.Owner.Events.OnTalkNpcGroupMade += Quest.Owner.Quests.OnTalkNpcGroupMadeHandler;

                            Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Event: 'OnTalkNpcGroupMade', Handler: 'OnTalkNpcGroupMadeHandler'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                    case "QuestActObjExpressFire":
                        {
                            //// нужно посмотреть в инвентарь, так как ещё не знаем, есть предмет в инвентаре или нет
                            //// we need to look in the inventory, because we don't know yet if the item is in the inventory or not
                            //res = CheckCount(act);
                            ////result2 = act.Template.IsCompleted();
                            //if (res)
                            //{
                            //    results2.Add(true); // уже выполнили задание, выход
                            //    break;
                            //}

                            // подписка одна на всех
                            Quest.Owner.Events.OnExpressFire -= Quest.Owner.Quests.OnExpressFireHandler;
                            Quest.Owner.Events.OnExpressFire += Quest.Owner.Quests.OnExpressFireHandler;

                            Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Event: 'OnExpressFire', Handler: 'OnExpressFireHandler'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                    case "QuestActObjAggro":
                        {
                            //// нужно посмотреть в инвентарь, так как ещё не знаем, есть предмет в инвентаре или нет
                            //// we need to look in the inventory, because we don't know yet if the item is in the inventory or not
                            //res = CheckCount(act);
                            ////result2 = act.Template.IsCompleted();
                            //if (res)
                            //{
                            //    results2.Add(true); // уже выполнили задание, выход
                            //    break;
                            //}

                            // подписка одна на всех
                            Quest.Owner.Events.OnAggro -= Quest.Owner.Quests.OnAggroHandler;
                            Quest.Owner.Events.OnAggro += Quest.Owner.Quests.OnAggroHandler;

                            Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Event: 'OnAggro', Handler: 'OnAggroHandler'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                    case "QuestActObjAbilityLevel":
                        {
                            //// нужно посмотреть в инвентарь, так как ещё не знаем, есть предмет в инвентаре или нет
                            //// we need to look in the inventory, because we don't know yet if the item is in the inventory or not
                            //res = CheckCount(act);
                            ////result2 = act.Template.IsCompleted();
                            //if (res)
                            //{
                            //    results2.Add(true); // уже выполнили задание, выход
                            //    break;
                            //}

                            // подписка одна на всех
                            Quest.Owner.Events.OnAbilityLevelUp -= Quest.Owner.Quests.OnAbilityLevelUpHandler;
                            Quest.Owner.Events.OnAbilityLevelUp += Quest.Owner.Quests.OnAbilityLevelUpHandler;

                            Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Event: 'OnAbilityLevelUp', Handler: 'OnAbilityLevelUpHandler'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                    case "QuestActObjCraft":
                        {
                            // нужно посмотреть в инвентарь, так как ещё не знаем, есть предмет в инвентаре или нет
                            // we need to look in the inventory, because we don't know yet if the item is in the inventory or not
                            res = CheckCount(act);
                            //result2 = act.Template.IsCompleted();
                            if (res)
                            {
                                results2.Add(true); // уже выполнили задание, выход
                                break;
                            }

                            // подписка одна на всех
                            Quest.Owner.Events.OnCraft -= Quest.Owner.Quests.OnCraftHandler;
                            Quest.Owner.Events.OnCraft += Quest.Owner.Quests.OnCraftHandler;

                            Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Event: 'OnCraft', Handler: 'OnCraftHandler'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                    case "QuestActObjCompleteQuest":
                        {
                            //Quest.Owner.Events.OnItemGather += Quest.Owner.Quests.OnItemGatherHandler;
                            //result2 = false;
                            break;
                        }
                }
            }
        }

        if (results2.All(b => b == true))
        {
            Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}. Подписываться на событие не надо, так как в инвентаре уже лежать нужные вещи.");
            Quest.ComponentId = CurrentComponent.Id;
            Quest.Status = QuestStatus.Ready;
            //Quest.Step = QuestComponentKind.Progress;
            Quest.Condition = QuestConditionObj.Ready;
            Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");

            // нужен здесь
            Quest.Owner.SendPacket(new SCQuestContextUpdatedPacket(Quest, Quest.ComponentId));

            return true;
        }

        Quest.ComponentId = 0;
        Quest.Condition = QuestConditionObj.Progress;
        Logger.Info($"[QuestProgressState][Update] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");

        // не нужен здесь
        //Quest.Owner.SendPacket(new SCQuestContextUpdatedPacket(Quest, Quest.ComponentId));

        return false;
    }

    private bool CheckCount(IQuestAct act)
    {
        // нужно посмотреть в инвентарь, так как ещё не знаем, есть предмет в инвентаре или нет
        // we need to look in the inventory, because we don't know yet if the item is in the inventory or not
        var objectiveCount = 0;
        switch (act.DetailType)
        {
            case "QuestActObjMonsterHunt":
                {
                    //var template = act.GetTemplate<QuestActObjMonsterHunt>(); // для доступа к переменным требуется привидение к нужному типу
                    //objectiveCount = Quest.Owner.Inventory.GetItemsCount(template.NpcId);
                    break;
                }
            case "QuestActObjItemGather":
                {
                    var template = act.GetTemplate<QuestActObjItemGather>(); // для доступа к переменным требуется привидение к нужному типу
                    objectiveCount = Quest.Owner.Inventory.GetItemsCount(template.ItemId);
                    break;
                }
            case "QuestActObjItemUse":
                {
                    //var template = act.GetTemplate<QuestActObjItemUse>(); // для доступа к переменным требуется привидение к нужному типу
                    //objectiveCount = Quest.Owner.Inventory.GetItemsCount(template.ItemId);
                    break;
                }
            case "QuestActObjTalk":
                {
                    var template = act.GetTemplate<QuestActObjTalk>(); // для доступа к переменным требуется привидение к нужному типу
                    objectiveCount = Quest.Owner.Inventory.GetItemsCount(template.ItemId);
                    break;
                }
            case "QuestActObjCraft":
                {
                    var template = act.GetTemplate<QuestActObjCraft>(); // для доступа к переменным требуется привидение к нужному типу
                    objectiveCount = Quest.Owner.Inventory.GetItemsCount(template.CraftId);
                    break;
                }
        }

        //var objective = act.Template.GetCount();
        var result = CurrentQuestComponent.Execute(Quest.Owner, Quest, objectiveCount).Any(b => b == true);
        return result;
    }

    private bool CheckResults(QuestState context, bool successive, bool selective, int count, bool letItDone, int score, EventArgs eventArgs)
    {
        //if (eventArgs == null) { return false; }
        //var ThisIsNotWhatYouNeed = new List<int>();
        //for (var i = 0; i < count; i++)
        //{
        //    ThisIsNotWhatYouNeed.Add(0);
        //}

        var results = false;
        var componentIndex = 0;

        foreach (var component in context.CurrentComponents)
        {
            var complete = false;
            Quest.ComponentId = component.Id;
            var acts = QuestManager.Instance.GetActs(component.Id);
            foreach (var act in acts)
            {
                if (Quest.ProgressStepResults[componentIndex])
                {
                    Logger.Info($"Quest: {Quest.TemplateId}, Step={Step}, checking the act {act.DetailType} gave the result {complete}.");
                    continue; // уже выполнен компонент
                }

                complete = CheckAct(component, act, componentIndex);

                Quest.ProgressStepResults[componentIndex] = complete;

                Logger.Info($"Quest: {Quest.TemplateId}, Step={Step}, checking the act {act.DetailType} gave the result {complete}.");
                // check the results for validity
                if (successive)
                {
                    // пока не знаю для чего это
                    // don't know what it's for yet
                    results = true;
                    Logger.Info($"Quest: {Quest.TemplateId}, Step={Step}, something was successful Successive={successive}.");
                }
                else if (selective && Quest.ProgressStepResults.Any(b => b))
                {
                    // разрешается быть подходящим одному предмету из нескольких
                    // it is allowed to be matched to one item out of several
                    results = true;
                    Logger.Info($"Quest: {Quest.TemplateId}, Step={Step}, allows you to make a choice Selective={selective}.");
                }
                else if (complete && count == 1 && !letItDone)
                {
                    // состоит из одного компонента и он выполнен
                    results = true;
                    Logger.Info($"Quest: {Quest.TemplateId}, Step={Step}, the only one stage completed with the result {results}.");
                }
                else if (complete && score == 0 && count > 1 && !letItDone)
                {
                    // Должны быть выполнены все компоненты
                    // All components must be executed
                    results = Quest.ProgressStepResults.All(b => b);
                    Logger.Info($"Quest: {Quest.TemplateId}, Step={Step}, stage {componentIndex} all stages completed with result {results}.");
                }
                else if (complete && score == 0 && componentIndex == count - 1 && count > 1 && !letItDone)
                {
                    // выполнен последний компонент из нескольких
                    // the last component of several components is executed
                    results = true;
                    Logger.Info($"Quest: {Quest.TemplateId}, Step={Step}, last {componentIndex} stage completed with result {results}.");
                }
                else if (Quest.OverCompletionPercent >= score && score != 0 && !letItDone)
                {
                    // выполнен один компонент из нескольких
                    results = true;
                    Logger.Info($"Quest: {Quest.TemplateId}, Step={Step}, OverCompletionPercent component {componentIndex} with result {results}.");
                }
                else if (complete)
                {
                    results = true;
                    Logger.Info($"Quest: {Quest.TemplateId}, Step={Step}, completed component {componentIndex} with result {results}.");
                }
            }

            componentIndex++;
        }
        return results;

        bool CheckAct(QuestComponent component, IQuestAct act, int idx)
        {
            //if (eventArgs == null) { return false; }

            switch (act.DetailType)
            {
                case "QuestActObjInteraction":
                    {
                        if (eventArgs is not OnInteractionArgs args) { return false; }
                        var template = act.GetTemplate<QuestActObjInteraction>(); // для доступа к переменным требуется привидение к нужному типу
                        // сначала проверим, что этотот Npc, может быть не тот, что надо по квесту
                        if (template?.DoodadId != args.DoodadId) { return false; }
                        break;
                    }
                case "QuestActObjMonsterHunt":
                    {
                        if (eventArgs is not OnMonsterHuntArgs args) { return false; }
                        var template = act.GetTemplate<QuestActObjMonsterHunt>(); // для доступа к переменным требуется привидение к нужному типу
                        // сначала проверим, что убили того Npc, может быть не тот, что надо по квесту
                        if (template?.NpcId != args.NpcId) { return false; }
                        break;
                    }
                case "QuestActObjMonsterGroupHunt":
                    {
                        if (eventArgs is not OnMonsterGroupHuntArgs args) { return false; }
                        var template = act.GetTemplate<QuestActObjMonsterGroupHunt>(); // для доступа к переменным требуется привидение к нужному типу
                        // сначала проверим, что убили того Npc, может быть не тот, что надо по квесту
                        if (!QuestManager.Instance.CheckGroupNpc(template.QuestMonsterGroupId, args.NpcId)) { return false; }
                        break;
                    }
                case "QuestActObjItemUse":
                    {
                        if (eventArgs is not OnItemUseArgs args) { return false; }
                        var template = act.GetTemplate<QuestActObjItemUse>(); // для доступа к переменным требуется привидение к нужному типу
                        // сначала проверим, что там использовали, может быть не то, что надо по квесту
                        if (template?.ItemId != args.ItemId) { return false; }
                        break;
                    }
                case "QuestActObjItemGroupUse":
                    {
                        if (eventArgs is not OnItemGroupUseArgs args) { return false; }
                        var template = act.GetTemplate<QuestActObjItemGroupUse>(); // для доступа к переменным требуется привидение к нужному типу
                        // сначала проверим, что там использовали, может быть не то, что надо по квесту
                        if (!QuestManager.Instance.CheckGroupItem(template.ItemGroupId, args.ItemGroupId)) { return false; }
                        break;
                    }
                case "QuestActObjItemGather":
                    {
                        var template = act.GetTemplate<QuestActObjItemGather>(); // для доступа к переменным требуется привидение к нужному типу
                        if (eventArgs == null)
                        {
                            // сюда попадаем сразу после шага Supply, поэтому событие не инициировано
                            var objectiveCount = Quest.Owner.Inventory.GetItemsCount(template.ItemId);
                            return act.Use(Quest.Owner, Quest, objectiveCount); // return the result of the check
                        }
                        // сначала проверим, что там подобрали, может быть не то, что надо по квесту
                        if (eventArgs is not OnItemGatherArgs args) { return false; }
                        if (template?.ItemId != args.ItemId)
                        {
                            Logger.Info($"[OnItemGatherHandler] Quest={Quest.TemplateId}. Это предмет {args.ItemId} не тот, что нужен нам {template?.ItemId}.");
                            return false;
                        }
                        break;
                    }
                case "QuestActObjItemGroupGather":
                    {
                        if (eventArgs is not OnItemGroupGatherArgs args) { return false; }
                        var template = act.GetTemplate<QuestActObjItemGroupGather>(); // для доступа к переменным требуется привидение к нужному типу
                        // сначала проверим, что там подобрали, может быть не то, что надо по квесту
                        if (!QuestManager.Instance.CheckGroupItem(template.ItemGroupId, args.ItemId))
                        {
                            Logger.Info($"[OnItemGatherHandler] Quest={Quest.TemplateId}. Это предмет {args.ItemId} не тот, что нужен нам {template.ItemGroupId}.");
                            return false;
                        }
                        break;
                    }
                case "QuestActObjZoneKill":
                    {
                        if (eventArgs is not OnZoneKillArgs args) { return false; }
                        var template = act.GetTemplate<QuestActObjZoneKill>(); // для доступа к переменным требуется привидение к нужному типу
                        // сначала проверим, может быть не то, что надо по квесту
                        if (template.ZoneId != args.ZoneId) { return false; }
                        break;
                    }
                case "QuestActObjZoneMonsterHunt":
                    {
                        if (eventArgs is not OnZoneMonsterHuntArgs args) { return false; }
                        var template = act.GetTemplate<QuestActObjZoneMonsterHunt>(); // для доступа к переменным требуется привидение к нужному типу
                        // сначала проверим, может быть не то, что надо по квесту
                        if (template.ZoneId != args.ZoneId) { return false; }
                        break;
                    }
                case "QuestActObjSphere":
                    {
                        if (eventArgs is not OnEnterSphereArgs args) { return false; }
                        //var template = act.GetTemplate<QuestActObjSphere>(); // для доступа к переменным требуется привидение к нужному типу
                        // сначала проверим, может быть не то, что надо по квесту
                        if (component.Id != args.SphereQuest.ComponentId) { return false; }
                        break;
                    }
                case "QuestActObjCraft":
                    {
                        if (eventArgs is not OnCraftArgs args) { return false; }
                        var template = act.GetTemplate<QuestActObjCraft>(); // для доступа к переменным требуется привидение к нужному типу
                        // сначала проверим, может быть не то, что надо по квесту
                        if (template.CraftId != args.CraftId) { return false; }
                        break;
                    }
                case "QuestActObjLevel":
                    {
                        if (eventArgs is not OnLevelUpArgs args) { return false; }
                        var template = act.GetTemplate<QuestActObjLevel>(); // для доступа к переменным требуется привидение к нужному типу
                        // сначала проверим, может быть не то, что надо по квесту
                        if (template.Level >= Quest.Owner.Level) { return false; }
                        break;
                    }
                case "QuestActObjAbilityLevel":
                    {
                        //if (eventArgs is not OnAbilityLevelUpArgs args) { return false; }
                        //var template = act.GetTemplate<QuestActObjAbilityLevel>(); // для доступа к переменным требуется привидение к нужному типу
                        //// сначала проверим, может быть не то, что надо по квесту
                        //if (template.Level >= Owner.Level) { return false; }
                        break;
                    }
                case "QuestActObjExpressFire":
                    {
                        if (eventArgs is not OnExpressFireArgs args) { return false; }
                        var template = act.GetTemplate<QuestActObjExpressFire>(); // для доступа к переменным требуется привидение к нужному типу
                        // сначала проверим, может быть не то, что надо по квесту
                        if (template.ExpressKeyId != args.EmotionId) { return false; }
                        break;
                    }
                case "QuestActObjAggro":
                    {
                        if (eventArgs is not OnAggroArgs args) { return false; }
                        var template = act.GetTemplate<QuestActObjAggro>(); // для доступа к переменным требуется привидение к нужному типу
                        // сначала проверим, может быть не то, что надо по квесту
                        if (MathUtil.CalculateDistance(Quest.Owner.Transform.World.Position, args.Transform.World.Position) > template.Range) { return false; }
                        break;
                    }
                case "QuestActObjTalk":
                    {
                        if (eventArgs is not OnTalkMadeArgs args) { return false; }
                        var template = act.GetTemplate<QuestActObjTalk>(); // для доступа к переменным требуется привидение к нужному типу
                        // сначала проверим, может быть не то, что надо по квесту
                        if (template?.NpcId != args.NpcId) { return false; }
                        break;
                    }
                case "QuestActObjTalkNpcGroup":
                    {
                        if (eventArgs is not OnTalkNpcGroupMadeArgs args) { return false; }
                        var template = act.GetTemplate<QuestActObjTalkNpcGroup>(); // для доступа к переменным требуется привидение к нужному типу
                        // сначала проверим, может быть не то, что надо по квесту
                        if (template.NpcGroupId != args.NpcGroupId) { return false; }
                        break;
                    }
                case "QuestActConReportNpc":
                    {
                        if (eventArgs is not OnReportNpcArgs args) { return false; }
                        var template = act.GetTemplate<QuestActConReportNpc>(); // для доступа к переменным требуется привидение к нужному типу
                        // сначала проверим, может быть не то, что надо по квесту
                        if (template?.NpcId != args.NpcId) { return false; }
                        break;
                    }
                case "QuestActConReportDoodad":
                    {
                        if (eventArgs is not OnReportDoodadArgs args) { return false; }
                        var template = act.GetTemplate<QuestActConReportDoodad>(); // для доступа к переменным требуется привидение к нужному типу
                        // сначала проверим, может быть не то, что надо по квесту
                        if (template?.DoodadId != args.DoodadId) { return false; }
                        break;
                    }
            }

            //Quest.Objectives[idx]++; // increase objective
            return act.Use(Quest.Owner, Quest, Quest.Objectives[idx]); // return the result of the check
        }
    }

    public override bool Complete(int selected = 0, EventArgs eventArgs = null)
    {

        var results = CheckResults(this, Quest.Template.Successive, Quest.Template.Selective, CurrentComponents.Count, Quest.Template.LetItDone, Quest.Template.Score, eventArgs);
        if (results)
        {
            Logger.Info($"[QuestProgressState][Complete] Quest: {Quest.TemplateId}. Шаг успешно завершен!");
            return true;
        }

        Logger.Info($"[QuestProgressState][Complete] Quest: {Quest.TemplateId}. Не все компоненты выполнены!");
        return false;
    }
    public override void Fail()
    {
        Logger.Info($"[QuestProgressState][Fail] Quest: {Quest.TemplateId} провален");
        Logger.Info($"[QuestProgressState][Fail] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
    }
    public override void Drop()
    {
        Logger.Info($"[QuestProgressState][Drop] Quest: {Quest.TemplateId} сброшен");
        Logger.Info($"[QuestProgressState][Drop] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
    }
}
public class QuestReadyState : QuestState
{
    public override bool Start(bool forcibly = false)
    {
        Logger.Info($"[QuestReadyState][Start] Quest: {Quest.TemplateId} уже в процессе выполнения.");
        Logger.Info($"[QuestReadyState][Start] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
        return true;
    }
    public override bool Update()
    {
        CurrentComponent = CurrentQuestComponent.GetFirstComponent();

        Logger.Info($"[QuestReadyState][Update] Quest: {Quest.TemplateId}. Подписываемся на события, которые требуются для активных актов");

        var results2 = new List<bool>();

        foreach (var component in CurrentComponents)
        {
            var acts = QuestManager.Instance.GetActs(component.Id);

            foreach (var act in acts)
            {
                switch (act?.DetailType)
                {
                    case "QuestActConReportNpc":
                        {
                            Quest.ReadyToReportNpc = true;
                            Quest.Owner.Events.OnReportNpc += Quest.Owner.Quests.OnReportNpcHandler;
                            Logger.Info($"[QuestReadyState][Update] Quest: {Quest.TemplateId}, Event: 'OnReportNpc', Handler: 'OnReportNpcHandler'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                    case "QuestActConReportDoodad":
                        {
                            Quest.Owner.Events.OnReportDoodad += Quest.Owner.Quests.OnReportDoodadHandler;
                            Logger.Info($"[QuestReadyState][Update] Quest: {Quest.TemplateId}, Event: 'OnReportDoodad', Handler: 'OnReportDoodadHandler'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                    case "QuestActConReportJournal":
                        {
                            //Quest.Owner.Events.OnReportJournal += Quest.Owner.Quests.OnReportJournalHandler;
                            Logger.Info($"[QuestReadyState][Update] Quest: {Quest.TemplateId}, Event: 'OnReportJournal', Handler: 'OnReportJournalHandler'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                    case "QuestActConAutoComplete":
                        {
                            //Quest.Owner.Events.OnQuestComplete += Quest.Owner.Quests.OnQuestCompleteHandler;
                            Logger.Info($"[QuestReadyState][Update] Quest: {Quest.TemplateId}, Event: 'OnQuestComplete', Handle: 'OnEventsOnQuestComplete'");
                            results2.Add(false); // будем ждать события
                            break;
                        }
                }
            }
        }

        if (results2.All(b => b == true))
        {
            Quest.ComponentId = CurrentComponent.Id;
            Quest.Status = QuestStatus.Ready;
            //Quest.Step = QuestComponentKind.Ready;
            Quest.Condition = QuestConditionObj.Ready;
            Logger.Info($"[QuestReadyState][Update] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");

            // нужен здесь
            Quest.Owner.SendPacket(new SCQuestContextUpdatedPacket(Quest, Quest.ComponentId));

            return true;
        }

        Quest.ComponentId = 0;
        //Quest.Step = QuestComponentKind.Ready;
        Logger.Info($"[QuestReadyState][Update] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");

        // не нужен здесь
        //Quest.Owner.SendPacket(new SCQuestContextUpdatedPacket(Quest, Quest.ComponentId));

        return false;
    }
    public override bool Complete(int selected = 0, EventArgs eventArgs = null)
    {
        //Quest.Step = QuestComponentKind.Ready;
        Logger.Info($"[QuestReadyState][Complete] Quest: {Quest.TemplateId}. Шаг успешно завершен!");
        Logger.Info($"[QuestReadyState][Complete] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");

        return true;
    }
    public override void Fail()
    {
        Logger.Info($"[QuestReadyState][Fail] Квест {Quest.TemplateId} провален");
        Logger.Info($"[QuestReadyState][Fail] Квест {Quest.TemplateId} Подписываемся на события, которые требуются для активных актов");
        foreach (var act in CurrentActs)
        {
            switch (act?.DetailType)
            {
                case "QuestActConFail":
                    //Quest.Owner.Events.OnFail += Quest.OnFailHandler;
                    break;
            }
        }
    }
    public override void Drop()
    {
        Logger.Info($"[QuestReadyState][Drop] Квест {Quest.TemplateId} сброшен");
        Logger.Info($"[QuestReadyState][Drop] Квест {Quest.TemplateId} уже завершен. Нельзя провалить.");
    }
}
public class QuestRewardState : QuestState
{
    public override bool Start(bool forcibly = false)
    {
        Logger.Info($"[QuestRewardState][Start] Quest: {Quest.TemplateId}. Уже в процессе выполнения.");
        Logger.Info($"[QuestRewardState][Start] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
        return true;
    }
    public override bool Update()
    {
        Logger.Info($"[QuestRewardState][Update] Quest: {Quest.TemplateId}. Получаем бонусы.");

        // получение бонусов организовано в Quests.Complete
        //Quest.Step = QuestComponentKind.Reward;
        Logger.Info($"[QuestRewardState][Update] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");

        return true;
    }
    public override bool Complete(int selected = 0, EventArgs eventArgs = null)
    {
        Logger.Info($"[QuestRewardState][Complete] Quest: {Quest.TemplateId}. Завершаем квест.");
        Logger.Info($"[QuestRewardState][Complete] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");

        Quest.Owner.Quests.Complete(Quest.TemplateId, selected); // Завершаем квест
        //Quest.Step = QuestComponentKind.Reward;

        return true;
    }
    public override void Fail()
    {
        Logger.Info($"[QuestRewardState][Fail] Quest: {Quest.TemplateId} уже завершен. Нельзя провалить.");
        Logger.Info($"[QuestRewardState][Fail] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
    }
    public override void Drop()
    {
        Logger.Info($"[QuestRewardState][Drop] Quest: {Quest.TemplateId} уже завершен. Нельзя сбросить.");
        Logger.Info($"[QuestRewardState][Drop] Quest: {Quest.TemplateId}, Character {Quest.Owner.Name}, ComponentId {Quest.ComponentId}, Step {Step}, Status {Quest.Status}, Condition {Quest.Condition}");
    }
}
