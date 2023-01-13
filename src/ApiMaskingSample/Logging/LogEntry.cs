using System.Collections.Concurrent;

namespace Microsoft.Extensions.Logging;

/// <summary>
/// </summary>
public class LogEntry : ConcurrentDictionary<string, object>
{
	protected string _msgKey;
	protected string _commentsKey;
	protected int _defaulCommentsCapacity = 3;

	public static LogEntry New(string msgKey = "message", string commentsKey = "comments")
	{
		return new LogEntry(msgKey, commentsKey);
	}

	public LogEntry(string msgKey = "message", string commentsKey = "comments") //: base()
	{
		_msgKey = msgKey ?? "message";
		_commentsKey = commentsKey ?? "comments";
	}

	/// <summary>
	/// not thread safe
	/// </summary>
	/// <param name="message"></param>
	/// <returns></returns>
	public LogEntry SetMessage(string message, params object[] args)
	{
		this[_msgKey] = string.Format(message ?? "", args);
		return this;
	}

	public LogEntry AddComment(string comment, params object[] args)
	{
		if (string.IsNullOrEmpty(comment)) return this;

		var cmt = string.Format(comment ?? "", args);
		AddOrUpdate(_commentsKey, new List<string> { cmt }, (key, value) =>
		{
			var commentList = (value as List<string>);
			if (commentList != null) commentList.Add(cmt);
			return value;
		});

		return this;
	}

	public LogEntry AddComments(IEnumerable<string> comments)
	{
		if (!comments.Any()) return this;

		AddOrUpdate(_commentsKey, new List<string>(comments), (key, value) =>
		{
			var commentList = (value as List<string>);
			if (commentList != null) commentList.AddRange(comments);
			return value;
		});

		return this;
	}

	public LogEntry AddKvp(string key, object value)
	{
		TryAdd(key, value);
		return this;
	}

	public LogEntry AddKvpIfNotNull(string key, object value)
	{
		if (value != null)
			TryAdd(key, value);
		return this;
	}

	public LogEntry AddKvpIfNotEmpty(string key, string value)
	{
		if (string.IsNullOrEmpty(value))
			TryAdd(key, value);
		return this;
	}

	public LogEntry AddKvpIf(Func<bool> validator, string key, object value)
	{
		if (validator())
			TryAdd(key, value);
		return this;
	}

	public LogEntry AddKvpIf(bool add, string key, object value)
	{
		if (add)
			TryAdd(key, value);
		return this;
	}
}
