

namespace Services.IntergralServer
{
    /// <summary>
    /// 公共返回数据类
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    public class CommonResponseDto<TOut>
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 提示消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public TOut Data { get; set; }
        /// <summary>
        /// 构建失败返回
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static CommonResponseDto<TOut> BuildError(string code, string message)
        {
            return new CommonResponseDto<TOut>()
            {
                Code = code,
                Message = message,
                Data = default
            };
        }
        /// <summary>
        /// 构建成功返回
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static CommonResponseDto<TOut> BuildOK(TOut data = default)
        {
            return new CommonResponseDto<TOut>()
            {
                Code = "200",
                Message = "",
                Data = data
            };
        }
    }
}
