<h1>Service Bus - Agendamento de Mensagens</h1>

<p>O `Service Bus` permite realizar o agendamento de mensagens. Com isso, ao enviar uma mensagem para o `Service Bus`, indicamos para ele que a referente mensagem será habilitada para os ouvintes somente na data de agendamento.</p>
<p>Quando a data de agendamento for atingida, a mensagem em questão se torna visível para os consumidores.</p>
<p>Além disso, caso haja a necessidade de cancelar o agendamento de uma mensagem, é possível fazer essa solicitação.</p>

<h2>Contexto</h2>
<p>Esse processo de agendamento é bem simples. Sendo assim, pode se tornar útil em alguns cenários, evitando criar um serviço de scheduler para esse gerenciamento.</p>
<p>Como exemplo, essa funcionalidade pode ser implementada para enviar um alerta ao usuário sobre um determinado evento quando o mesmo estiver próximo.</p>

<h2>Exemplo</h2>
<p>Neste exemplo, além de utilizar o `Service Bus` para o agendamento das mensagens, foi utilizada uma tabela para armazená-los, a fim gerenciar os agendamentos.</p>

<h3>Service Bus - Criando mensagem agendada</h3>

```csharp
public async Task<MessageBusScheduleRequest<TContent>.Response> ScheduleMessageAsync<TContent>(MessageBusScheduleRequest<TContent> request, CancellationToken cancellationToken)
{
  var sender = _client.CreateSender(request.QueueName);
  var message = new ServiceBusMessage(JsonSerializer.Serialize(request.Payload));
  var messageNumber = await sender.ScheduleMessageAsync(message, TimeZoneInfo.ConvertTimeToUtc(request.ScheduleTo.Value), cancellationToken);
  await sender.CloseAsync(cancellationToken).ConfigureAwait(false);

  return new()
  {
      MessageNumber = messageNumber,
  };
}
```

<p>O realizar o agendamento da mensagem, é retornar um identificador referente a mensagem criada. Esse identificador será necessário para caso seja solicitado o cancelamento do agendamento.</p>

<h3>Service Bus - Cancelando mensagem agendada</h3>

```csharp
public async Task CancelScheduleMessageAsync(MessageBusCancelScheduledRequest request, CancellationToken cancellationToken)
{
  var sender = _client.CreateSender(request.QueueName);
  await sender.CancelScheduledMessageAsync(request.MessageNumber, cancellationToken);
  await sender.CloseAsync(cancellationToken).ConfigureAwait(false);    
}
```

<p>Para cancelar a mensagem agendada, é necessário informar qual o identificador da mesma.</p>
