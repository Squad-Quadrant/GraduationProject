using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PurpleFlowerCore.PFCDebug
{
    

    public class DebugMenu : MonoBehaviour, IDragHandler
    {
        //todo:UI框架
        [SerializeField] private GameObject menu;
        [SerializeField] private Transform itemRoot;
        [SerializeField] private LogPanel logPanel;
        [SerializeField] private DebugInput debugInput;
        
        private readonly Tree<CommandBase> _commandTree = new();
        // private readonly Dictionary<string,DebugCommandInfo> _menuCommands = new();
        private readonly List<DebugMenuItem> _items = new();
        private TreeNode<CommandBase> _currentNode;
        // [Inspectable]private int _currentPathIndex = 0;
        private const string ItemUIPath = "PFCRes/DebugMenuItem";
        
        public void AddCommand(string commandPath, Action command)
        {
            _commandTree.CreateNodeByPath(commandPath, CommandFactory.CreateCommand(command));
        }
        
        public void AddCommand<T>(string commandPath, Action<T> command)
        {
            _commandTree.CreateNodeByPath(commandPath, CommandFactory.CreateCommand(command));
        }
        
        public void AddCommand(string commandPath, Action<object> command, Type type)
        {
            _commandTree.CreateNodeByPath(commandPath, CommandFactory.CreateCommand(command));
        }
        
        private void Awake()
        {
            _currentNode = _commandTree.Root;
        }

        public void ClickItem(TreeNode<CommandBase> commandNode)
        {
            if (commandNode.IsLeaf)
            {
                if(commandNode.Value.ParamType == null)
                {
                    PFCLog.Debug("DebugMenu", "execute command:" + commandNode.name);
                    commandNode.Value?.Invoke(null);
                }
                else
                {
                    debugInput.Show(commandNode);
                }
            }
            else
            {
                _currentNode = commandNode;
                Refresh();
            }
        }

        public void Input(string input)
        {
            if(string.IsNullOrEmpty(input)) return;
            string[] inputs = input.Split(' ');
            var commands = _commandTree.GetLeaves();
            foreach (var command in commands)
            {
                if (command.name != inputs[0]) continue;
                if(command.Value.ParamType != null)
                {
                    if(inputs.Length > 1)
                    {
                        PFCLog.Info("DebugMenu", "execute command:" + command.name + ' ' + inputs[1]);
                        command.Value.Invoke(inputs[1]);
                    }
                    else
                    {
                        PFCLog.Error("DebugMenu","command:"+command.name+" need a param");
                    }
                }
                else
                {
                    command.Value.Invoke(null);
                    PFCLog.Info("DebugMenu", "execute command:" + command.name);
                }
                return;
            }
            PFCLog.Debug(input);
        }

        private void ClearItems()
        {
            foreach (var item in _items)
            {
                Destroy(item.gameObject);
            }
            _items.Clear();
        }
        
        public void Back()
        {
            if (_currentNode.IsRoot) return;
            _currentNode = _currentNode.Parent;
            Refresh();
        }
        
        private void ShowItem(TreeNode<CommandBase> commandNode)
        {
            var newItem = Instantiate(Resources.Load<DebugMenuItem>(ItemUIPath), itemRoot);
            _items.Add(newItem);
            newItem.Init(commandNode, this);
        }

        private void ShowItems()
        {
            foreach (var node in _currentNode.Children)
            {
                ShowItem(node);
            }
        }
        
        public void Switch()
        {
            Switch(!menu.activeSelf);
        }

        public void Switch(bool open)
        {
            menu.SetActive(open);
            if (open)
            {
                Refresh();
            }
        }
        
        public void Refresh()
        {
            ClearItems();
            ShowItems();
        }
        
        public void Log(LogData data)
        {
            logPanel.Print(data);
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position += (Vector3)eventData.delta;
        }
    }
}