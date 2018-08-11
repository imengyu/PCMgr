.model flat
.data
extern _PsGetNextProcessThread:dword

.code

_KxGetNextProcessThread_x86Call@8 PROC

Process= dword ptr  8
Thread= dword ptr  0Ch

push    ebp
mov     ebp, esp
mov     ebx, [ebp+Thread]
push    ebx
mov     eax, [ebp+Process]
call    _PsGetNextProcessThread
mov     ebx, eax
mov     eax, ebx
pop     ebp
retn    8
_KxGetNextProcessThread_x86Call@8 ENDP

END