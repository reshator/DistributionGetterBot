﻿using DistributionGetterBot.Handlers;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#nullable disable
using CancellationTokenSource cts = new();

TelegramBotClient client = new TelegramBotClient(Environment.GetEnvironmentVariable("TOKEN")!);
client.StartReceiving(UpdateHandler, ErrorHandler, cancellationToken: cts.Token);

Console.ReadLine();
cts.Cancel();

async Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
	var ErrorMessage = exception switch
	{
		ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
		_ => exception.ToString()
	};
	await Task.Run(() => Console.WriteLine(ErrorMessage));
}

async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
	try
	{
		Console.WriteLine(update.Message.From.Id);
		await (update.Type switch
		{
			UpdateType.Message => update.Message.Text! switch
			{
				"/start" => MessagesHandler.GetStarted(botClient, update.Message.Chat.Id, update.Message.From),
				"/help" => MessagesHandler.GetHelp(botClient, update.Message.Chat.Id),
				string item when item.ToLower().StartsWith("/dist") => MessagesHandler.GetDistribution(botClient, update.Message.Chat.Id, update.Message.Text),
				"" => Task.CompletedTask,
				_ => Task.CompletedTask
			},
			UpdateType.InlineQuery => Task.CompletedTask,
			UpdateType.CallbackQuery => Task.CompletedTask,
			_ => Task.CompletedTask
		});
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Exception while handling: {update.Type}: {ex}");
	}
}

