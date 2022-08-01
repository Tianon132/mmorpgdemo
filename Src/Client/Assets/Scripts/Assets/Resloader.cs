﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//用于自动更新提前准备的，属于资源加载类
class Resloader
{
    public static T Load<T>(string path) where T : UnityEngine.Object
    {
        return Resources.Load<T>(path);
    }
}