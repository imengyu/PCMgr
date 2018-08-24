.data


.code

MGetK32ModuleHandleCore PROC

  xor rdx, rdx             ; Zero rdx
  mov rax, [gs:30h]  ; RAX points to TEB (Thread Environment Block) 
  mov rcx,[rax+60h] ; RCX points to PEB (Process Environment Block)
  mov rdx, rcx     ; Get a pointer to the PEB
  mov rdx, [rdx+24]        ; Get PEB->Ldr
  mov rdx, [rdx+32]        ; Get the first module from the InMemoryOrder module list
next_mod:                  ;
  mov rsi, [rdx+80]        ; Get pointer to modules name (unicode string)
  movzx rcx, word [rdx+74] ; Set rcx to the length we want to check 
  xor r9, r9               ; Clear r9 which will store the hash of the module name
loop_modname:              ;
  xor rax, rax             ; Clear rax
  lodsb                    ; Read in the next byte of the name
  cmp al, 'a'              ; Some versions of Windows use lower case module names
  jl not_lowercase         ;
  sub al, 0x20             ; If so normalise to uppercase
not_lowercase:             ;
  ror r9d, 13              ; Rotate right our hash value
  add r9d, eax             ; Add the next byte of the name
  loop loop_modname        ; Loop untill we have read enough
  ; We now have the module hash computed
  push rdx                 ; Save the current position in the module list for later
  push r9                  ; Save the current module hash for later
  ; Proceed to itterate the export address table, 
  mov rdx, [rdx+32]        ; Get this modules base address 
  mov rax,rdx

MGetK32ModuleHandleCore ENDP

END