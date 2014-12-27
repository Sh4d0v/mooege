#include <idc.idc>
 
static ReadPushOperand( xref, filter )
{
    do
    {
        auto disasm;
        disasm = GetDisasm( xref );
 
        if ( strstr( disasm, "push" ) > -1 && strstr( substr(disasm, 4, -1), filter ) > -1 )
        {
            break;
        }
       
        xref = PrevHead( xref, PrevFunction( xref ) );
    } while ( 1 );
 
    return GetOperandValue( xref, 0 );
}
 
static ReadSize( xref )
{
    return ReadPushOperand( xref, "h" );
}
 
static ReadAddr( xref )
{
    return ReadPushOperand( xref, "offset" );
}
 
static DumpProtoBins( funcOffset )
{
    auto xref, count, i, file;
    xref = RfirstB( funcOffset );
 
    do
    {
        auto size, addr;
 
        size = ReadSize( xref );
        addr = ReadAddr( xref );
 
        file = fopen(form("protobin%u.protobin", count), "wb");
 
        Message( "file: %s size: 0x%04X addr: 0x%08X\n",form("protobin%u.protobin", count), size, addr );
        count++;
        for(i = 0; i < size; ++i)
        {
            fputc(Byte(addr + i), file);
        }
        fclose(file);
    } while ( (xref = RnextB( funcOffset, xref )) != -1 );
}
 
static main()
{
  auto x;
 
   // DescriptorPool::InternalAddGeneratedFile
   // Find function with string:
   // CHECK failed: generated_database_->Add(encoded_file_descriptor, size): 
   //old search
   //x = FindBinary(0, SEARCH_DOWN, "55 8B EC 6A FF 68 ? ? ? ? 64 A1 00 00 00 00 50 83 EC ? A1 ? ? ? ? 33 C5 ? 8D 45 F4 64 A3 00 00 00 00 C7 45 C4 00 00 00 00 E8 ? ? ? ?");
   
   //1.0.5
   //Diablo iii.exe
   //x = FindBinary(0, SEARCH_DOWN, "55 8B EC 6A FF 68 09 C3 38 01 64 A1 00 00 00 00 50 83 EC 34 A1 90 CC 69 01 33 C5 89 45 F0 56 50");
   //battlenet.dll
   //x = FindBinary(0, SEARCH_DOWN, "55 8B EC 6A FF 68 69 7F D6 3C 64 A1 00 00 00 00 50 83 EC 34 A1 C0 E9 ED 3C 33 C5 89 45 F0 56 50");
   //1.0.7
   //diablo iii.exe
   //x = FindBinary(0, SEARCH_DOWN, "55 8B EC 6A FF 68 ? ? ? ? 64 A1 00 00 00 00 50 83 EC 34 A1 ? ? ? ? 33 C5 89 45 F0 56 50");
   //battle.dll
   //x = FindBinary(0, SEARCH_DOWN, "55 8B EC 6A FF 68 99 7C  D7 3C 64 A1 00 00 00 00 50 83 EC 34 A1 20 2B EF 3C 33 C5 89 45 F0 56 50");
   //1.0.8
   //diablo iii.exe
   //x = FindBinary(0, SEARCH_DOWN, "55 8B EC 6A FF 68 19 20 3E 01 64 A1 00 00 00 00 50 83 EC 34 A1 00 45 6F 01 33 C5 89 45 F0 56 50");
   //battle.dll
   x = FindBinary(0, SEARCH_DOWN, "55 8B EC 6A FF 68 E9 CF E1 3C 64 A1 00 00 00 00 50 83 EC 34 A1 90 2B F9 3C 33 C5 89 45 F0 56 50");
 
  if ( x == BADADDR )
  {
    Message("Can't find DescriptorPool::InternalAddGeneratedFile, aborting...\n");
    return -1;
  }
 
  Message("%X\n", x);
  DumpProtoBins(x);
}