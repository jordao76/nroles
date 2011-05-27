using System.Collections.Generic;

namespace NRoles.Engine {
  
  sealed partial class MemberComposer : IMessageContainer, Does<RMessageContainer> {

    public extern IEnumerable<Message> Messages { [Placeholder] get; }
    [Placeholder] public extern void AddMessage(Message message);

  }

}
