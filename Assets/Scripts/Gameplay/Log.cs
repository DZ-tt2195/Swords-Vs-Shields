using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using Photon.Pun;
using System.Linq.Expressions;
using System;
using System.Linq;
using TMPro;

public class DecisionContainer
{
    public List<RollBack> listOfRBs { get; private set; }
    public List<DecisionContainer> listOfDCs { get; private set; }
    private bool _complete;
    public bool complete
    {
        get
        {
            return _complete;
        }
        set
        {
            if (value != _complete)
            {
                _complete = value;
                if (_complete)
                    Log.inst.completedDecisions.Add(this);
                else
                    Log.inst.completedDecisions.Remove(this);
            }
        }
    }

    public PhotonCompatible source { get; private set; }
    public Expression<Action> action { get; private set; }
    public int logged { get; private set; }
    public bool forward { get; private set; }
    public DecisionContainer parent;
    public string actionName { get; private set; }
    //public string parameters;

    public DecisionContainer(DecisionContainer parentContainer, PhotonCompatible source, int logged, bool forward, Expression<Action> action)
    {
        this.complete = !forward;
        listOfDCs = new();
        listOfRBs = new();
        this.source = source;
        this.action = action;
        this.logged = logged;
        parent = parentContainer;
        parentContainer?.listOfDCs.Add(this);
        this.forward = forward;

        if (action != null)
        {
            (string instruction, object[] parameters) = this.source.TranslateFunction(action);
            actionName = instruction;
        }
        /*
        string answer = $"";
        foreach (object obj in parameters)
            answer += $"{obj}, ";
        this.parameters = answer;*/
    }
}

public class RollBack
{
    public PhotonCompatible source { get; private set; }
    public Expression<Action> action { get; private set; }
    public DecisionContainer parent { get; private set; }
    public string actionName { get; private set; }
    //public string parameters;

    public RollBack(DecisionContainer parentContainer, PhotonCompatible source, Expression<Action> action)
    {
        this.source = source;
        this.action = action;
        parent = parentContainer;
        parentContainer?.listOfRBs.Add(this);
        /*
        (string instruction, object[] parameters) = this.source.TranslateFunction(action);
        actionName = instruction;
        string answer = $"";
        foreach (object obj in parameters)
            answer += $"{obj}, ";
        this.parameters = answer;*/
    }
}

public class LogText
{
    public string text { get; private set; }
    public int indent { get; private set; }
    public bool important { get; private set; }
    public DecisionContainer undoToThis { get; private set; }

    public LogText(string text, int indent, bool important)
    {
        this.text = text;
        this.indent = indent;
        this.important = important;
    }

    public void AssignDecisionContainer(DecisionContainer undo)
    {
        this.undoToThis = undo;
    }
}

public class Log : PhotonCompatible
{

#region Variables

    public static Log inst;
    public float waitTime { get; private set; }

    [Foldout("Texts", true)]
    [SerializeField] Scrollbar scroll;
    [SerializeField] TMP_Text allPast;
    [SerializeField] TMP_Text importantPast;

    List<LogText> currentLogTexts = new();
    [SerializeField] TMP_Text allCurrent;
    [SerializeField] TMP_Text importantCurrent;

    [Foldout("Current step", true)]
    public List<Action> inReaction = new();
    public DecisionContainer currentContainer { get; private set; }
    public CoroutineGroup groupToWait;

    [Foldout("Decisions", true)]
    public List<DecisionContainer> completedDecisions = new();
    List<DecisionContainer> initialContainers = new();
    public List<LogText> undosInLog = new();
    [SerializeField] Button undoButton;
    [SerializeField] Slider logTypeSlider;
    bool storeUndoPoint = false;

    protected override void Awake()
    {
        base.Awake();
        this.bottomType = this.GetType();
        inst = this;
        undoButton.onClick.AddListener(() => InvokeUndo());
        undoButton.gameObject.SetActive(false);
        groupToWait = new(this);
        logTypeSlider.onValueChanged.AddListener(LogToggle);
        void LogToggle(float value)
        {
            ChangeScrolling();
        }
    }

    #endregion

#region Add To Log

    public static string Article(string followingWord)
    {
        if (followingWord.StartsWith('A')
            || followingWord.StartsWith('E')
            || followingWord.StartsWith('I')
            || followingWord.StartsWith('O')
            || followingWord.StartsWith('U'))
        {
            return $"an {followingWord}";
        }
        else
        {
            return $"a {followingWord}";
        }
    }

    public void AddMyText(string logText, bool important, int indent = 0, bool isUndo = true)
    {
        if (indent >= 0)
            NewRollback(this, () => AddToCurrent(false, logText, indent, important, isUndo));
    }

    void AddToCurrent(bool undo, string logText, int indent, bool important, bool isUndo)
    {
        if (undo)
        {
            int count = currentLogTexts.Count-1;
            LogText nextText = currentLogTexts[count];
            currentLogTexts.RemoveAt(count);

            RemoveText(allCurrent);
            if (nextText.important)
                RemoveText(importantCurrent);

            void RemoveText(TMP_Text textBox)
            {
                int lastNewline = textBox.text.LastIndexOf('\n');
                allCurrent.text = textBox.text[..lastNewline];
            }
        }
        else
        {
            LogText saveText = new(logText, indent, important);
            string targetText = $"\n{Translator.inst.SplitAndTranslate(PhotonNetwork.LocalPlayer.ActorNumber, logText, indent)}";
            allCurrent.text += targetText;
            if (important)
                importantCurrent.text += targetText;

            if (currentContainer != null && isUndo && storeUndoPoint)
            {
                if (currentContainer.action != null)
                {
                    saveText.AssignDecisionContainer(currentContainer);
                    undosInLog.Insert(0, saveText);
                }
                storeUndoPoint = false;
            }
        }
    }

    public void MasterText(string logText)
    {
        if (AmMaster())
            DoFunction(() => AddToPast(-1, logText, 0, true), RpcTarget.AllBuffered);
    }

    public void ShareTexts()
    {
        foreach (LogText nextLog in currentLogTexts)
            DoFunction(() => AddToPast(PhotonNetwork.LocalPlayer.ActorNumber, nextLog.text, nextLog.indent, nextLog.important));

        allCurrent.text = "";
        importantCurrent.text = "";
        currentLogTexts.Clear();
        initialContainers.Clear();
        completedDecisions.Clear();
        undosInLog.Clear();
    }

    [PunRPC]
    void AddToPast(int owner, string logText, int indent, bool important)
    {
        string targetText = $"\n{Translator.inst.SplitAndTranslate(owner, logText, indent)}";
        allPast.text += targetText;
        if (important)
            importantPast.text += targetText;
    }

    void ChangeScrolling()
    {
        if (logTypeSlider.value == 1)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(allPast.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(allCurrent.rectTransform);
        }
        else
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(importantPast.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(importantCurrent.rectTransform);
        }
        Invoke(nameof(ScrollDown), 0.2f);
    }

    void ScrollDown()
    {
        if (scroll.value <= 0.1f)
            scroll.value = 0;
    }
    /*
    void OnEnable()
    {
        Application.logMessageReceived += DebugMessages;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= DebugMessages;
    }

    void DebugMessages(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error)
        {
            AddText($"");
            AddText($"the game crashed :(");
        }
    }
    */
    #endregion

#region Undos

    private void Update()
    {
        undoButton.gameObject.SetActive(undosInLog.Count > 0);
    }

    void ClearCurrentDecision(bool clear)
    {
        ChangeScrolling();
        scroll.value = 0;
        storeUndoPoint = false;
        inReaction.Clear();

        Popup[] allPopups = FindObjectsByType<Popup>(FindObjectsSortMode.None);
        foreach (Popup popup in allPopups)
        {
            if (popup.beDestroyed)
                Destroy(popup.gameObject);
        }
        SliderChoice[] allSliders = FindObjectsByType<SliderChoice>(FindObjectsSortMode.None);
        foreach (SliderChoice slider in allSliders)
        {
            if (slider.beDestroyed)
                Destroy(slider.gameObject);
        }
        ButtonSelect[] allSelectables = FindObjectsByType<ButtonSelect>(FindObjectsSortMode.None);
        foreach (ButtonSelect select in allSelectables)
        {
            select.button.interactable = false;
            select.button.onClick.RemoveAllListeners();
            select.border.gameObject.SetActive(false);
        }

        if (currentContainer != null && currentContainer.parent != null && clear)
        {
            int index = currentContainer.parent.listOfDCs.IndexOf(currentContainer);
            if (index == 0)
            {
                currentContainer.parent.listOfDCs.Clear();
                Debug.Log($"parent DCs cleared");
            }
        }
        currentContainer = null;
    }

    void InvokeUndo()
    {
        DecisionContainer toThisPoint = undosInLog[^1].undoToThis;
        ClearCurrentDecision(true);

        for (int i = completedDecisions.Count - 1; i >= 0; i--)
        {
            //Debug.Log($"{i} vs {completedDecisions.Count}");
            DecisionContainer container = completedDecisions[i];
            container.complete = false;
            //Debug.Log($"{container.actionName} is undone, {container.complete}");

            for (int j = container.listOfRBs.Count - 1; j >= 0; j--)
            {
                RollBack next = container.listOfRBs[j];
                (string instruction, object[] parameters) = next.source.TranslateFunction(next.action);
                object[] newParameters = parameters.ToArray();
                newParameters[0] = true;
                next.source.StringParameters(instruction, newParameters);
                container.listOfRBs.RemoveAt(j);
            }

            if (!container.forward)
            {
                completedDecisions.Remove(container);
                container.parent.listOfDCs.Remove(container);
            }

            if (container == toThisPoint)
            {
                //Debug.Log($"continue at {toThisPoint.actionName}, {toThisPoint.logged}, {toThisPoint.complete}");
                PopStack();
                return;
            }
            else if (container.parent != null)
            {
                int index = container.parent.listOfDCs.IndexOf(container);
                if (index == 0)
                {
                    container.parent.listOfDCs.Clear();
                    Debug.Log($"parent DCs cleared");
                }
            }
        }
    }

    #endregion

#region New Steps

    public RollBack NewRollback(PhotonCompatible source, Expression<Action> action)
    {
        RollBack next = new(currentContainer, source, action);
        action.Compile().Invoke();
        return next;
    }

    public DecisionContainer NewDecisionContainer(PhotonCompatible source, Expression<Action> action, int logged)
    {
        DecisionContainer next = new(currentContainer, source, logged, true, action);
        if (currentContainer == null)
            initialContainers.Add(next);
        return next;
    }

    /*
    public List<NextStep> SearchHistory(string name)
    {
        List<NextStep> hasStepName = new();
        foreach (NextStep step in historyStack)
        {
            if(step.actionName.Equals(name))
                hasStepName.Add(step);
        }
        return hasStepName;
    }
    */
    #endregion

#region Choice Resolve

    public void SetUndoPoint(bool apply)
    {
        storeUndoPoint = apply;
    }

    public void PopStack(bool run = true)
    {
        List<Action> newActions = new();
        for (int i = 0; i < inReaction.Count; i++)
            newActions.Add(inReaction[i]);

        inReaction.Clear();
        foreach (Action action in newActions)
            action?.Invoke();

        if (currentContainer != null)
        {
            currentContainer.complete = true;
            //Debug.Log($"{currentContainer.actionName} is complete");
        }
        StartCoroutine(WaitAndContinue());

        IEnumerator WaitAndContinue()
        {
            float time = 0f;
            int coroutines = groupToWait.ActiveCoroutinesAmount;
            if (coroutines > 0)
            {
                while (groupToWait.AnyProcessing)
                {
                    time += Time.deltaTime;
                    yield return null;
                }
                Debug.Log($"did {coroutines} coroutines in {time:F2} sec");
            }

            ChangeScrolling();
            currentContainer = null;
            storeUndoPoint = false;

            foreach (var KVP in PlayerCreator.inst.playerDictionary)
                PlayerCreator.inst.playerDictionary[KVP.Key].UpdateUI();

            foreach (DecisionContainer container in initialContainers)
                Iterate(container);

            void Iterate(DecisionContainer container)
            {
                if (!container.complete)
                {
                    if (currentContainer == null)
                    {
                        //Debug.Log($"{container.actionName}{container.logged} first assigned");
                        currentContainer = container;
                    }
                    else
                    {
                        //Debug.Log($"{container.actionName}({container.logged}) vs {currentContainer.actionName}({currentContainer.logged})");
                        if (container.logged > currentContainer.logged)
                            currentContainer = container;
                    }
                }
                else
                {
                    foreach (DecisionContainer nextContainer in container.listOfDCs)
                    {
                        Iterate(nextContainer);
                    }
                }
            }

            //HandleUI();
            if (currentContainer != null && run)
            {
                if (currentContainer.action == null)
                {
                    PopStack(run);
                }
                else
                {
                    //Debug.Log($"forward: {currentContainer.actionName}");
                    currentContainer.action.Compile().Invoke();
                    if (storeUndoPoint == false)
                        PopStack(run);
                }
            }
        }
    }

    #endregion

}
