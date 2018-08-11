
.data
extern PsGetNextProcessThread:qword

.code


KxGetNextProcessThread_x64Call PROC

arg_0= qword ptr  8
arg_8= qword ptr  10h

mov     [rsp+arg_8], rdx
mov     [rsp+arg_0], rcx
sub     rsp, 28h
mov     rdx, [rsp+28h+arg_8]
mov     rcx, [rsp+28h+arg_0]
mov     rbx, rdx
call PsGetNextProcessThread
mov     rbx, rax
mov     rax, rbx
add     rsp, 28h
ret
KxGetNextProcessThread_x64Call ENDP

END