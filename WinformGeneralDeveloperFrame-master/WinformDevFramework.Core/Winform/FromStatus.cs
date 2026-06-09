

namespace WinformDevFramework.Core.Winform
{
    public enum FormStatus
    {
        View,//双击 新增 编辑 删除 详情页不可编辑
        Add,//保存 撤销 不可切换页签 详情页可编辑  详情页清空
        Edit,//保存 撤销 不可切换页签 详情页可编辑
        First,//新增 详情页不可编辑
        Canel,//页签可切换 新增 编辑 详情页不可编辑
        Save,//页签可切换 详情页不可编辑 编辑 新增 删除
        Del, //页签可切换 详情页不可编辑 新增
    }
}
