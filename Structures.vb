
''' <summary>
''' Structures class contains the entire file data structure of statement file 
''' </summary>
''' <remarks>
''' Created : 2015-10-28
''' </remarks>
Public Class Structures

#Region " Customer Statement File Structure "

    ''' <summary>
    ''' Statement File (StmFil) - Structure
    ''' </summary>
    ''' <remarks>
    ''' Created : 2015-10-28
    ''' </remarks>
    Public Structure StmFil_Struct
        Public Header As FileHeaderRecord_Struct
        Public Footer As FileFooterRecord_Struct
    End Structure

    Public Structure FileHeaderRecord_Struct
        Public RecTyp As String                     ' Desc: 16x	M	Extract Type, constant “PP STMT Data”
        Public FilDte As String                     ' Desc: 8n	M	File creation date YYYYMMDD

        Public Function ReadRecord(ByRef Record As String) As Boolean
            Try
                If Record Is Nothing OrElse Record.Length = 0 Then
                    Return False
                End If

                RecTyp = ServicesUti.Services.Singleton.AppInstance.strip(Record, 16)
                FilDte = ServicesUti.Services.Singleton.AppInstance.strip(Record, 8)
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function
    End Structure

    Public Structure FileFooterRecord_Struct
        Public Label As String                     ' Desc: Document Type (always STMT)
        Public NumRec As String                     ' Desc: File identity (file number)
       
        Public Function ReadRecord(ByRef Record As String) As Boolean
            Try
                If Record Is Nothing OrElse Record.Length = 0 Then
                    Return False
                End If

                Label = ServicesUti.Services.Singleton.AppInstance.strip(Record, 9)
                NumRec = ServicesUti.Services.Singleton.AppInstance.strip(Record, 10)
                Return True

            Catch ex As Exception
                Return False
            End Try

        End Function

    End Structure
   
#End Region
#Region "Travel Card/Shopping Card (POSCusStm) Structure "

    Public Structure CurrencyList
        Public CcyCde As String
        Public CcyNum As String
        Public CcyDscEng As String
        Public CcyDscAra As String
        Public CcyExp As Integer
    End Structure

    Public Structure POSSCSTBalance
        Dim CusNum As String
        Dim WalId As String
        Dim Ccy As String
        Dim AccountId As String
        Dim LedgerBalance As String
        Dim AvailableBalance As String
        Dim PreviousMonthEnd As Date
        Public Sub clear()
            CusNum = ""
            WalId = ""
            Ccy = ""
            AccountId = ""
            LedgerBalance = ""
            AvailableBalance = ""
        End Sub

    End Structure
    ''' <summary>
    ''' Transactions - Structure
    ''' The transaction record(s) varies statement to statement and may or may not occur in any statement.
    ''' </summary>
    ''' <remarks>
    ''' Created : 2018-10-28
    ''' </remarks>
    Public Structure POSSCSTCusStm_Trx

        Public CusNum As String
        Public WalId As String
        Public TrxTyp As String 'currency code
        Public PAN As String
        Public AccountId As String
        Public TransactioType As String
        Public TrxDteGrg As String                  ' Desc: Transaction post date in Gregorian (‘dd mmm yy’) 
        Public TranAmount As String
        Public SettlementAmount As String
        Public TrxIndicator As String
        Public TxnCurrency As String
        Public SettleCurrencyCode As String
        Public TrxDatetime As DateTime
        Public STAN As String
        Public RRN As String
        Public MCC As String
        Public AuthId As String
        Public MerchantID As String
        Public MerchantNameLOC As String
        Public AB As String
        Public LB As String
        Public FixFee As String
        Public FeePercentage As String
        Public TotalFee As String
        Public TxnISOCurrency As String
        Public SettleISOCurrencyCode As String
        Public TxnCurrencyName As String
        Public SettleCurrencyName As String
        Public PostDatetime As DateTime

        Public Sub clear()
            CusNum = ""
            WalId = ""
            TrxTyp = "" 'currency code
            PAN = ""
            AccountId = ""
            TransactioType = ""
            TrxDteGrg = ""
            TranAmount = ""
            SettlementAmount = ""
            TrxIndicator = ""
            TxnCurrency = ""
            SettleCurrencyCode = ""
            'TrxDatetime = ""
            STAN = ""
            RRN = ""
            MCC = ""
            AuthId = ""
            MerchantID = ""
            MerchantNameLOC = ""
            AB = ""
            LB = ""
            FixFee = ""
            FeePercentage = ""
            TotalFee = ""
            TxnISOCurrency = ""
            SettleISOCurrencyCode = ""
            TxnCurrencyName = ""
            SettleCurrencyName = ""
        End Sub
    End Structure

    Public Structure POSSCSTCusWallet
        Dim CusNum As String
        Dim WalId As String
        Dim WalNikNam As String
        Dim CurInWal As String
        Dim CreLim As String

        Public Sub clear()
            CusNum = ""
            WalId = ""
            WalNikNam = ""
            CurInWal = ""
            CreLim = ""
        End Sub

    End Structure

    Public Structure POSSCCusStm_Struct
        Public SHD As CusDBStm_Hdr
        Public TRX() As POSSCSTCusStm_Trx
        Public Balance() As POSSCSTBalance
    End Structure

    Public Structure POSTCCusStm_Struct
        Public SHD As CusDBStm_Hdr
        Public WHD() As POSSCSTCusWallet
        Public TRX() As POSSCSTCusStm_Trx
        Public Balance() As POSSCSTBalance
    End Structure


    Public Structure POSCusStm_Struct
        Public SHD As CusDBStm_Hdr
        Public TRX As POSCusStm_Trx
    End Structure

    ''' <summary>
    ''' Transactions - Structure
    ''' The transaction record(s) varies statement to statement and may or may not occur in any statement.
    ''' </summary>
    ''' <remarks>
    ''' Created : 2018-10-28
    ''' </remarks>
    Public Structure POSCusStm_Trx
        Public TrxDteGrg As String                  ' Desc: Transaction post date in Gregorian (‘dd mmm yy’) 
       
        Public AccountId As String
        Public TransactioType As String
        Public TranAmount As String
        Public SettlementAmount As String
        Public TrxIndicator As String
        Public TxnCurrency As String
        Public SettleCcurrencyCode As String
        Public TrxDatetime As String
        Public STAN As String
        Public RRN As String
        Public MCC As String
        Public AuthId As String
        Public MerchantID As String
        Public MerchantNameLOC As String
        Public AB As String
        Public LB As String
        Public FixFee As String
        Public FeePercentage As String
        Public TotalFee As String

    End Structure

    ''' <summary>
    ''' Statement Header (SHD) [200] - Structure
    ''' The statement header record must occur only once and must be the first record in a statement group of records.
    ''' </summary>
    ''' <remarks>
    ''' Created : 2018-10-28
    ''' </remarks>
    Public Structure POSCusStm_Hdrq
        Public StatementNumber As String
        Public StatmentDate As String
        Public CusNum As String
        Public CusSeg As String
        Public CusLng As String
        Public CusArTit As String
        Public CusArNam1 As String
        Public CusArNam2 As String
        Public CusArNam3 As String
        Public CusArNam4 As String
        Public CusEnTit As String
        Public CusEnNam1 As String
        Public CusEnNam2 As String
        Public CusEnNam3 As String
        Public CusEnNam4 As String
        Public CusArAddLin1 As String
        Public CusArAddLin2 As String
        Public CusArAddLin3 As String
        Public CusArAddLin4 As String
        Public CusEnAddLin1 As String
        Public CusEnAddLin2 As String
        Public CusEnAddLin3 As String
        Public CusEnAddLin4 As String
        Public NoOfTrvWal As String
        Public NoOfShpWal As String

        Public Function ReadRecord(Rec As String) As Boolean
            Try


                Return True
            Catch ex As Exception
                Return False
            End Try

        End Function
    End Structure
#End Region
#Region " Customer Statement (CusStm) Structure "

   
    Public Structure CusStm_Struct
        Public SHD As CusStm_Hdr
    End Structure

    Public Structure CusDBStm_Hdr

        Public StatementNumber As String
        Public StatmentDate As String
        Public CusNum As String
        Public CusSeg As String
        Public CusLng As String
        Public CusArTit As String
        Public CusArNam1 As String
        Public CusArNam2 As String
        Public CusArNam3 As String
        Public CusArNam4 As String
        Public CusEnTit As String
        Public CusEnNam1 As String
        Public CusEnNam2 As String
        Public CusEnNam3 As String
        Public CusEnNam4 As String
        Public CusArAddLin1 As String
        Public CusArAddLin2 As String
        Public CusArAddLin3 As String
        Public CusArAddLin4 As String
        Public CusEnAddLin1 As String
        Public CusEnAddLin2 As String
        Public CusEnAddLin3 As String
        Public CusEnAddLin4 As String
        Public NoOfTrvWal As String
        Public NoOfShpWal As String
        Public CusID As Integer
        Public Function Clear() As Boolean

            Try
                StatementNumber = ""
                StatmentDate = Format(gCurrentMonthEnd, "dd-MM-yyyy")
                CusNum = ""
                CusSeg = ""
                CusLng = ""
                CusArTit = ""
                CusArNam1 = ""
                CusArNam2 = ""
                CusArNam3 = ""
                CusArNam4 = ""
                CusEnTit = ""
                CusEnNam1 = ""
                CusEnNam2 = ""
                CusEnNam3 = ""
                CusEnNam4 = ""
                CusArAddLin1 = ""
                CusArAddLin2 = ""
                CusArAddLin3 = ""
                CusArAddLin4 = ""
                CusEnAddLin1 = ""
                CusEnAddLin2 = ""
                CusEnAddLin3 = ""
                CusEnAddLin4 = ""
                NoOfTrvWal = ""
                NoOfShpWal = ""
                CusID = 0
                Return True
            Catch ex As Exception
                Return False
            End Try

        End Function
    End Structure
    ''' <summary>
    ''' Statement Header (SHD) [200] - Structure
    ''' The statement header record must occur only once and must be the first record in a statement group of records.
    ''' </summary>
    ''' <remarks>
    ''' Created : 2018-10-28
    ''' </remarks>
    Public Structure CusStm_Hdr
        Public StatementNumber As String
        Public CusNum As String
        Public CutSeg As String
        Public CutLng As String
        Public CusArTit As String
        Public CusArNam1 As String
        Public CusArNam2 As String
        Public CusArNam3 As String
        Public CusArNam4 As String
        Public CusEnTit As String
        Public CusEnNam1 As String
        Public CusEnNam2 As String
        Public CusEnNam3 As String
        Public CusEnNam4 As String
        Public CusArAddLin1 As String
        Public CusArAddLin2 As String
        Public CusArAddLin3 As String
        Public CusArAddLin4 As String
        Public CusEnAddLin1 As String
        Public CusEnAddLin2 As String
        Public CusEnAddLin3 As String
        Public CusEnAddLin4 As String
        Public Rsv As String
        Public NoOfTrvWal As String
        Public NoOfShpWal As String
        Public TvlWalDet() As TravelCardWallet
        Public ShpWalDet() As ShoppingCardWallet

        Public Function ReadRecord(Rec As String) As Boolean
            Dim lNoOfTrvWal As Integer
            Dim lNoOfShpWal As Integer
            Dim i As Integer
            Try
                StatementNumber = "00001"

                CusNum = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 6)
                'If CusNum = "001555" Or CusNum = "012669" Then Stop
                CutSeg = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 1)
                CutLng = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 1)

                CusArTit = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 10).Trim
                CusArNam1 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 15).Trim
                CusArNam2 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 15).Trim
                CusArNam3 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 15).Trim
                CusArNam4 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 15).Trim


                CusEnTit = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 10).Trim
                CusEnNam1 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 15).Trim
                CusEnNam2 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 15).Trim
                CusEnNam3 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 15).Trim
                CusEnNam4 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 15).Trim

                CusArAddLin1 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 35).Trim
                CusArAddLin2 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 35).Trim
                CusArAddLin3 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 35).Trim
                CusArAddLin4 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 35).Trim





                'CusArNam3 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 15)
                'CusArNam2 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 15)
                'CusArNam4 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 15)
                'CusArNam1 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 15)

                'CusArTit = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 10)


                'CusEnTit = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 10)
                'CusEnNam1 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 15)
                'CusEnNam4 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 15)
                'CusEnNam2 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 15)
                'CusEnNam3 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 15)

                'CusArAddLin3 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 35)
                'CusArAddLin2 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 35)
                'CusArAddLin1 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 35)
                'CusArAddLin4 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 35)

                CusEnAddLin1 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 35).Trim
                CusEnAddLin2 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 35).Trim
                CusEnAddLin3 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 35).Trim
                CusEnAddLin4 = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 35).Trim

                Rsv = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 223)
                NoOfTrvWal = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 3)
                NoOfShpWal = ServicesUti.Services.Singleton.AppInstance.strip(Rec, 3)
                lNoOfTrvWal = CInt(NoOfTrvWal)
                ReDim TvlWalDet(lNoOfTrvWal - 1)
                lNoOfShpWal = CInt(NoOfShpWal)
                ReDim ShpWalDet(lNoOfShpWal - 1)
                For i = 0 To lNoOfTrvWal - 1
                    TvlWalDet(i).clear()

#If WalletFix Then
                    TvlWalDet(i).Fill(ServicesUti.Services.Singleton.AppInstance.strip(Rec, 695))
#Else
TvlWalDet(i).Fill(ServicesUti.Services.Singleton.AppInstance.strip(Rec, 215))
#End If


                Next
                For i = 0 To lNoOfShpWal - 1
                    ShpWalDet(i).clear()
                    ShpWalDet(i).Fill(ServicesUti.Services.Singleton.AppInstance.strip(Rec, 98))
                Next
                Return True
            Catch ex As Exception
                Return False
            End Try

        End Function
    End Structure

    Public Structure Postilian_Trx
        Public CusNo As String
        Public WalletId As String
        Public TrxType As String
        Public PAN As String
        Public AccountId As String
        Public TransactioType As String
        Public TranAmount As String
        Public SettlementAmount As String
        Public TrxIndicator As String
        Public TxnCurrency As String
        Public SettleCcurrencyCode As String
        Public TrxDatetime As String
        Public STAN As String
        Public RRN As String
        Public MCC As String
        Public AuthId As String
        Public MerchantID As String
        Public MerchantNameLOC As String
        Public AB As String
        Public LB As String
        Public FixFee As String
        Public FeePercentage As String
        Public TotalFee As String

        Public Sub Clear()
            CusNo = String.Empty
            WalletId = String.Empty
            TrxType = String.Empty
            PAN = String.Empty
            AccountId = String.Empty
            TransactioType = String.Empty
            TranAmount = String.Empty
            SettlementAmount = String.Empty
            TrxIndicator = String.Empty
            TxnCurrency = String.Empty
            SettleCcurrencyCode = String.Empty
            'TrxDatetime = String.Empty
            STAN = String.Empty
            RRN = String.Empty
            MCC = String.Empty
            AuthId = String.Empty
            MerchantID = String.Empty
            MerchantNameLOC = String.Empty
            AB = String.Empty
            LB = String.Empty
            FixFee = String.Empty
            FeePercentage = String.Empty
            TotalFee = String.Empty

        End Sub


    End Structure

    Public Structure TravelCardWallet
        Dim WalId As String
        Dim WalNikNam As String
        Dim CurInWal As String
        Dim CreLim As String
        Dim Rsv1 As String

        Public Sub Fill(rec As String)
            Dim WalIdStr As String
            Dim WalIdNum As Integer = 0

            WalIdStr = ServicesUti.Services.Singleton.AppInstance.strip(rec, 3)
            If WalIdStr.Length > 0 And IsNumeric(WalIdStr) Then
                WalIdNum = CInt(WalIdStr)
                WalIdNum = WalIdNum - 1
                If WalIdNum < 0 Then WalIdNum = 0
                'WalIdNum = WalIdStr.Length - 1
            End If

            WalId = Format(WalIdNum, "#000")
            WalNikNam = ServicesUti.Services.Singleton.AppInstance.strip(rec, 30)


#If WalletFix Then
            CurInWal = ServicesUti.Services.Singleton.AppInstance.strip(rec, 600)
#Else
            CurInWal = ServicesUti.Services.Singleton.AppInstance.strip(rec, 120)
#End If



            CreLim = ServicesUti.Services.Singleton.AppInstance.strip(rec, 12)
            Rsv1 = ServicesUti.Services.Singleton.AppInstance.strip(rec, 50)
        End Sub
        Public Sub clear()
            WalId = ""
            WalNikNam = ""
            CurInWal = ""
            CreLim = ""
            Rsv1 = ""
        End Sub

    End Structure

    Public Structure ShoppingCardWallet
        Dim ShpWalId As String
        Dim ShpWalNikNam As String
        Dim ShpCurInWal As String
        Dim ShpCreLim As String
        Dim Rsv2 As String

        Public Sub Fill(rec As String)
            Dim WalIdStr As String
            Dim WalIdNum As Integer = 0
            WalIdStr = ServicesUti.Services.Singleton.AppInstance.strip(rec, 3)
            If WalIdStr.Length > 0 And IsNumeric(WalIdStr) Then
                WalIdNum = CInt(WalIdStr)
                WalIdNum = WalIdNum - 1
                If WalIdNum < 0 Then WalIdNum = 0
                'WalIdNum = WalIdStr.Length - 1
            End If
            ShpWalId = Format(WalIdNum, "#000")
            ShpWalNikNam = ServicesUti.Services.Singleton.AppInstance.strip(rec, 30)
            ShpCurInWal = ServicesUti.Services.Singleton.AppInstance.strip(rec, 3)
#If WalletFix Then
            ShpCreLim = ServicesUti.Services.Singleton.AppInstance.strip(rec, 12)
#Else
             ShpCreLim = "000000002500" 
#End If

            Rsv2 = ServicesUti.Services.Singleton.AppInstance.strip(rec, 50)
        End Sub
        Public Sub clear()
            ShpWalId = ""
            ShpWalNikNam = ""
            ShpCurInWal = ""
            ShpCreLim = ""
            Rsv2 = ""
        End Sub
    End Structure

    


#End Region

End Class
