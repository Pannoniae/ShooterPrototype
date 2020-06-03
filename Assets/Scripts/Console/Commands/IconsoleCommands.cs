using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Console.Commands.IconsoleCommands
{
    public interface IConsoleCommand
    {
        string Word { get; }
        bool Process(string[]args);
    }
}
