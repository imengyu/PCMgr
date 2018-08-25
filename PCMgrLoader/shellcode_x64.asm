.data
.code

MGetCurrentPeb PROC
mov rbx, 0
mov rbx, GS:[60h]
mov rax, rbx
ret
MGetCurrentPeb ENDP

END