﻿using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
   
    public class SnakePart
    {
        public UIElement UiElement { get; set; }  
        public Point Position { get; set; }  +
        public bool IsHead { get; set; }  
}
