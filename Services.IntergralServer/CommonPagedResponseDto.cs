using System;



namespace Services.IntergralServer
{
    /// <summary>
    /// 公共返回分页数据类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommonPagedResponseDto<T> : CommonResponseDto<T>
    {
        /// <summary>
        /// 当前页码（默认：1）
        /// </summary>
        public int PageNumber { get; set; } = 1;
        /// <summary>
        /// 分页大小（默认：10，最大100）
        /// </summary>
        public int PageSize { get; set; } = 10;
        /// <summary>
        /// 数据总数
        /// </summary>
        public long DataCount { get; set; } = 0;
        /// <summary>
        /// 构建失败返回信息
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="pagenumber"></param>
        /// <param name="pagesize"></param>
        /// <param name="datacount"></param>
        /// <returns></returns>
        public static CommonPagedResponseDto<T> BuildError(string code, string message, int pagenumber = 1, int pagesize = 10, int datacount = 0)
        {
            return new CommonPagedResponseDto<T>()
            {
                Code = code,
                Message = message,
                Data = default,
                DataCount = datacount,
                PageNumber = pagenumber,
                PageSize = pagesize
            };
        }
        /// <summary>
        /// 构建成功返回信息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static CommonPagedResponseDto<T> BuildOK(T data = default, int pagenumber = 1, int pagesize = 10, long datacount = 0)
        {
            return new CommonPagedResponseDto<T>()
            {
                Code = "200",
                Message = "",
                Data = data,
                DataCount = datacount,
                PageNumber = pagenumber,
                PageSize = pagesize
            };
        }
    }
}
