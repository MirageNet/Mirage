using Mirage;

namespace SyncVarTests.SyncVarsMoreThan63
{
    class SyncVarsMoreThan63 : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnChangeHealth))]
        int health { get; set; }

        [SyncVar] int var2 { get; set; }
        [SyncVar] int var3 { get; set; }
        [SyncVar] int var4 { get; set; }
        [SyncVar] int var5 { get; set; }
        [SyncVar] int var6 { get; set; }
        [SyncVar] int var7 { get; set; }
        [SyncVar] int var8 { get; set; }
        [SyncVar] int var9 { get; set; }
        [SyncVar] int var10 { get; set; }
        [SyncVar] int var11 { get; set; }
        [SyncVar] int var12 { get; set; }
        [SyncVar] int var13 { get; set; }
        [SyncVar] int var14 { get; set; }
        [SyncVar] int var15 { get; set; }
        [SyncVar] int var16 { get; set; }
        [SyncVar] int var17 { get; set; }
        [SyncVar] int var18 { get; set; }
        [SyncVar] int var19 { get; set; }
        [SyncVar] int var20 { get; set; }
        [SyncVar] int var21 { get; set; }
        [SyncVar] int var22 { get; set; }
        [SyncVar] int var23 { get; set; }
        [SyncVar] int var24 { get; set; }
        [SyncVar] int var25 { get; set; }
        [SyncVar] int var26 { get; set; }
        [SyncVar] int var27 { get; set; }
        [SyncVar] int var28 { get; set; }
        [SyncVar] int var29 { get; set; }
        [SyncVar] int var30 { get; set; }
        [SyncVar] int var31 { get; set; }
        [SyncVar] int var32 { get; set; }
        [SyncVar] int var33 { get; set; }
        [SyncVar] int var34 { get; set; }
        [SyncVar] int var35 { get; set; }
        [SyncVar] int var36 { get; set; }
        [SyncVar] int var37 { get; set; }
        [SyncVar] int var38 { get; set; }
        [SyncVar] int var39 { get; set; }
        [SyncVar] int var40 { get; set; }
        [SyncVar] int var41 { get; set; }
        [SyncVar] int var42 { get; set; }
        [SyncVar] int var43 { get; set; }
        [SyncVar] int var44 { get; set; }
        [SyncVar] int var45 { get; set; }
        [SyncVar] int var46 { get; set; }
        [SyncVar] int var47 { get; set; }
        [SyncVar] int var48 { get; set; }
        [SyncVar] int var49 { get; set; }
        [SyncVar] int var50 { get; set; }
        [SyncVar] int var51 { get; set; }
        [SyncVar] int var52 { get; set; }
        [SyncVar] int var53 { get; set; }
        [SyncVar] int var54 { get; set; }
        [SyncVar] int var55 { get; set; }
        [SyncVar] int var56 { get; set; }
        [SyncVar] int var57 { get; set; }
        [SyncVar] int var58 { get; set; }
        [SyncVar] int var59 { get; set; }
        [SyncVar] int var60 { get; set; }
        [SyncVar] int var61 { get; set; }
        [SyncVar] int var62 { get; set; }
        [SyncVar] int var63 { get; set; }
        [SyncVar] int var64 { get; set; }

        public void TakeDamage(int amount)
        {
            if (!IsServer)
                return;

            health -= amount;
        }

        void OnChangeHealth(int oldHealth, int newHealth)
        {
            // do things with your health bar
        }
    }
}
