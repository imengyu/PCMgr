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
push     ebx
mov     eax, [ebp+Process]

call _PsGetNextProcessThread
pop     ebx

pop     ebp
ret 4

_KxGetNextProcessThread_x86Call@8 ENDP

END